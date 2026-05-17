using Godot;
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

    public override void _Ready()
    {
        CampaignState.EnsureInitialized();

        var startButton = GetNode<Button>("RootMargin/MainVBox/HeroPanel/HeroMargin/HeroVBox/PrimaryButtonRow/StartBattleButton");
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

        if (HasAutomationArg("--projectfr-autostart-battle"))
        {
            CallDeferred(MethodName.OnStartBattlePressed);
        }
    }

    private void RefreshMenu()
    {
        var mission = CampaignState.GetSelectedMission();
        _descriptionLabel.Text = "의뢰를 고르고 시스템에 침투해 자료 회수, 삭제, 정찰을 수행한 뒤 보상과 추적 리스크를 관리하는 해커 작전 게임.";
        _missionTitleLabel.Text = $"{mission.Title} · {mission.ClientName}";
        _missionBriefingLabel.Text = $"{mission.Briefing}\n\n목표: {mission.ObjectiveType} {mission.TargetPath}\n제한 턴: {mission.TurnLimit}";
        _missionRewardLabel.Text = $"성공 보상: {mission.RewardCredits} 크레딧 / 평판 +{mission.RewardReputation}\n실패 페널티: {mission.FailurePenaltyCredits} 크레딧 손실 / 추적도 +{mission.FailureHeat}";
        _operatorStatusLabel.Text = $"크레딧: {CampaignState.Credits}\n평판: {CampaignState.Reputation}\n추적도: {CampaignState.Heat}";

        var lastRun = CampaignState.LastMissionResult;
        _lastRunLabel.Text = lastRun == null
            ? "최근 작전: 아직 없음"
            : $"최근 작전: {(lastRun.Success ? "성공" : "실패")}\n{lastRun.Mission.Title}\n{lastRun.Summary}";
    }

    private void OnPreviousMissionPressed()
    {
        CampaignState.SelectPreviousMission();
        RefreshMenu();
    }

    private void OnNextMissionPressed()
    {
        CampaignState.SelectNextMission();
        RefreshMenu();
    }

    private void OnStartBattlePressed()
    {
        CampaignState.BeginSelectedMission();
        GetTree().ChangeSceneToFile("res://res/scenes/BattleScene.tscn");
    }

    private static bool HasAutomationArg(string arg)
    {
        return OS.GetCmdlineUserArgs().Contains(arg);
    }
}
