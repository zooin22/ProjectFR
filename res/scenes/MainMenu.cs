using Godot;
using ProjectFR.Core;
using ProjectFR.Mission;

namespace ProjectFR.Scenes;

public partial class MainMenu : Control
{
	private Label _descriptionLabel = null!;
	private Label _missionTitleLabel = null!;
	private Label _missionBriefingLabel = null!;
	private Label _missionRewardLabel = null!;
	private Label _operatorStatusLabel = null!;
	private Label _lastRunLabel = null!;
	private Button _startButton = null!;
	private bool _transitioning;

	public override void _Ready()
	{
		DebugLog.Initialize();
		DebugLog.Info(nameof(MainMenu), "_Ready enter");

		try
		{
			CampaignState.EnsureInitialized();

			var startButton = GetNode<Button>("RootMargin/MainVBox/HeroPanel/HeroMargin/HeroVBox/PrimaryButtonRow/StartBattleButton");
			_startButton = startButton;
			var previousMissionButton = GetNode<Button>("RootMargin/MainVBox/HeroPanel/HeroMargin/HeroVBox/PrimaryButtonRow/PreviousMissionButton");
			var nextMissionButton = GetNode<Button>("RootMargin/MainVBox/HeroPanel/HeroMargin/HeroVBox/PrimaryButtonRow/NextMissionButton");

			_descriptionLabel = GetNode<Label>("RootMargin/MainVBox/HeroPanel/HeroMargin/HeroVBox/DescriptionLabel");
			_missionTitleLabel = GetNode<Label>("RootMargin/MainVBox/InfoRow/MissionPanel/MissionMargin/MissionVBox/MissionTitleValueLabel");
			_missionBriefingLabel = GetNode<Label>("RootMargin/MainVBox/InfoRow/MissionPanel/MissionMargin/MissionVBox/MissionBriefingLabel");
			_missionRewardLabel = GetNode<Label>("RootMargin/MainVBox/InfoRow/MissionPanel/MissionMargin/MissionVBox/MissionRewardLabel");
			_operatorStatusLabel = GetNode<Label>("RootMargin/MainVBox/InfoRow/OperatorPanel/OperatorMargin/OperatorVBox/OperatorStatusLabel");
			_lastRunLabel = GetNode<Label>("RootMargin/MainVBox/InfoRow/OperatorPanel/OperatorMargin/OperatorVBox/LastRunLabel");

			startButton.Pressed += OnStartBattlePressed;
			previousMissionButton.Pressed += OnPreviousMissionPressed;
			nextMissionButton.Pressed += OnNextMissionPressed;

			RefreshMenu();
			DebugLog.Info(nameof(MainMenu), $"menu ready :: selected mission = {CampaignState.GetSelectedMission().Id}");

			if (HasAutomationArg("--projectfr-autostart-battle") || HasAutomationArg("--projectfr-smoke-test-menu-flow"))
			{
				DebugLog.Info(nameof(MainMenu), "automation -> start battle");
				CallDeferred(MethodName.OnStartBattlePressed);
			}
		}
		catch (Exception exception)
		{
			DebugLog.Exception(nameof(MainMenu), exception, "_Ready failed");
			throw;
		}
	}

	private void RefreshMenu()
	{
		var mission = CampaignState.GetSelectedMission();
		var modifiers = CampaignState.GetModifiers();
		var effectiveTurnLimit = Math.Max(3, mission.TurnLimit - modifiers.HeatTurnPenalty);
		_descriptionLabel.Text = "작전 로비에서 의뢰를 검토하고, 보상·위험도를 확인한 뒤 실제 폴더 던전 침투를 시작한다.";
		_missionTitleLabel.Text = $"{mission.Title} · {mission.Client.Name}";

		var briefing = $"세력: {mission.Client.Faction}\n브리핑: {mission.Briefing}\n\n목표: {mission.ObjectiveType} {mission.TargetPath}\n기본 제한 턴: {mission.TurnLimit} / 현재 제한 턴: {effectiveTurnLimit}\n의뢰 성향: {mission.Client.Agenda}\n리스크: {mission.Client.RiskNote}";
		var conflictNote = BuildConflictNote(mission);
		if (!string.IsNullOrEmpty(conflictNote))
			briefing += $"\n\n{conflictNote}";
		_missionBriefingLabel.Text = briefing;

		_missionRewardLabel.Text = $"성공 보상: {mission.RewardCredits} 크레딧 / 평판 +{mission.RewardReputation}\n실패 페널티: {mission.FailurePenaltyCredits} 크레딧 손실 / 추적도 +{mission.FailureHeat}\n현재 추적 보정: {modifiers.Summary}";

		var factionRep = CampaignState.GetFactionReputation(mission.Client.FactionId);
		var factionRepSign = factionRep >= 0 ? "+" : "";
		_operatorStatusLabel.Text = $"크레딧: {CampaignState.Credits}\n평판: {CampaignState.Reputation}\n추적도: {CampaignState.Heat}\n{mission.Client.Faction} 평판: {factionRepSign}{factionRep}\n적 ATK +{modifiers.EnemyAttackBonus} / 적 AP +{modifiers.EnemyApBonus} / 적 HP +{modifiers.EnemyHpBonus}";

		var lastRun = CampaignState.LastMissionResult;
		_lastRunLabel.Text = lastRun == null
			? "최근 작전: 아직 없음"
			: $"최근 작전: {(lastRun.Success ? "성공" : "실패")}\n{lastRun.Mission.Title}\n{lastRun.Summary}";
	}

	private void OnPreviousMissionPressed()
	{
		CampaignState.SelectPreviousMission();
		DebugLog.Info(nameof(MainMenu), $"select previous mission -> {CampaignState.GetSelectedMission().Id}");
		RefreshMenu();
	}

	private void OnNextMissionPressed()
	{
		CampaignState.SelectNextMission();
		DebugLog.Info(nameof(MainMenu), $"select next mission -> {CampaignState.GetSelectedMission().Id}");
		RefreshMenu();
	}

	private void OnStartBattlePressed()
	{
		if (_transitioning) return;
		_transitioning = true;
		_startButton.Disabled = true;
		DebugLog.Info(nameof(MainMenu), $"start battle pressed :: mission index resolved to {CampaignState.GetSelectedMission().Id}");
		CampaignState.BeginSelectedMission();
		CallDeferred(MethodName.ChangeToBattleScene);
	}

	private void ChangeToBattleScene()
	{
		DebugLog.Info(nameof(MainMenu), "ChangeSceneToFile -> BattleScene.tscn");
		GetTree().ChangeSceneToFile("res://res/scenes/BattleScene.tscn");
	}

	private static string BuildConflictNote(MissionData mission)
	{
		var conflicts = CampaignState.MissionBoard
			.Where(m => m.Id != mission.Id
				&& m.TargetPath == mission.TargetPath
				&& m.Client.FactionId != mission.Client.FactionId
				&& m.ObjectiveType != mission.ObjectiveType)
			.ToList();

		if (conflicts.Count == 0)
			return string.Empty;

		var lines = conflicts.Select(c =>
		{
			var status = CampaignState.IsMissionAvailable(c) ? "대기 중" : "잠김";
			return $"  · {c.Title} — {c.Client.Name} ({c.Client.Faction}) [{status}]";
		});

		return "[충돌] 같은 타깃에 대립하는 의뢰:\n" + string.Join("\n", lines);
	}

	private static bool HasAutomationArg(string arg)
	{
		return OS.GetCmdlineUserArgs().Contains(arg);
	}
}
