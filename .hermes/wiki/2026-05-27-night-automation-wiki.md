# ProjectFR Night Automation Wiki (Lite)

Updated: 2026-05-27 05:13:15 KST

## 최근 변경(요약)
- 2026-05-27 05:13:15 KST — autoheal 크론/스크립트 추가(에러 분류·자동복구·조치로그), night-pass.env 기반 안전모드 자동적용
- 2026-05-27 05:07:44 KST — night-review에서 Claude timeout(75s)을 실패로 누적하지 않도록 조정, failure streak 0으로 리셋
- 2026-05-27 04:53:39 KST — 위키 경량(Lite) 포맷 재구성
- 2026-05-27 04:52:34 KST — 불필요 섹션 정리
- 2026-05-27 04:51:02 KST — usage 임계치 문서화/연동

## 현재 운영 상태
- `deterministic-scheduler-watch` (`72ffc62b1370`)
  - every 1m / `deterministic_scheduler.py` / no_agent
- `projectfr-night-review` (`c858ff94ec41`)
  - `*/5 2-7 * * *` / `projectfr_night_review_claude.sh` / no_agent
- `projectfr-manual-mode-guard` (`963b927655bc`)
  - every 1m / `projectfr_cron_guard.py` / no_agent / deliver: local
- `projectfr-night-autoheal` (`5b2998cb1106`)
  - every 2m / `projectfr_night_autoheal.py` / no_agent

## 핵심 운영 포인트
- 실행 플로우: TODO 확인 → TODO 작업 → docs TODO 체크(토글) → TODO 0이면 리뷰
- 수동작업 ON 우선: manual mode ON이면 관련 잡 pause
- 사용량 가드: Claude 잔여율 임계치 기반 night-review pause/resume
  - pause: `<= 5%`
  - resume: `>= 20%`
  - 중간/측정불가: 직전 상태 유지

## 자주 쓰는 파일
- 가드 스크립트: `/home/zooin/.hermes/scripts/projectfr_cron_guard.py`
- 나이트 패스: `/home/zooin/.hermes/scripts/projectfr_night_review_claude.sh`
- 오토힐: `/home/zooin/.hermes/scripts/projectfr_night_autoheal.py`
- 임계치 고정값: `/mnt/f/Agent/hermes/Godot/projects/ProjectFR/.hermes/cron-guard.env`
- 런타임 안전모드 env: `/mnt/f/Agent/hermes/Godot/projects/ProjectFR/.hermes/night-pass.env`
- 수동모드 상태: `/mnt/f/Agent/hermes/Godot/projects/ProjectFR/.hermes/manual-work.json`
- 가드 상태: `/mnt/f/Agent/hermes/Godot/projects/ProjectFR/.hermes/cron-guard-state.json`
- 실행 로그: `/mnt/f/Agent/hermes/Godot/projects/ProjectFR/.hermes/night-pass-runs.jsonl`
- 오토힐 조치 로그: `/mnt/f/Agent/hermes/Godot/projects/ProjectFR/.hermes/night-autoheal-actions.jsonl`

## 운영 명령
- 수동모드 ON: `bash /home/zooin/.hermes/scripts/projectfr_manual_mode_on.sh 120 "manual-work"`
- 수동모드 OFF: `bash /home/zooin/.hermes/scripts/projectfr_manual_mode_off.sh`
- 오토힐 즉시 실행: `python3 /home/zooin/.hermes/scripts/projectfr_night_autoheal.py`
- 크론 재개:
  - `hermes cron resume 72ffc62b1370`
  - `hermes cron resume c858ff94ec41`

## 위키 기록 규칙 (Lite)
- 앞으로 이 문서는 경량 유지(운영에 필요한 정보만)
- 새 변경은 아래 `변경 이력`에 1~2줄로만 추가
- 형식: `YYYY-MM-DD HH:MM:SS KST — 변경요약`
- 수정 시 `Updated`도 함께 갱신

## 변경 이력
- 2026-05-27 04:53:39 KST — 위키를 경량(Lite) 운영 포맷으로 재구성
- 2026-05-27 04:52:34 KST — 불필요 섹션 정리(벤치마크/스킬 소개/과거 타임라인 제거)
- 2026-05-27 04:51:02 KST — Claude usage 임계치 고정값 문서화 및 guard env 파일 연동 추가
