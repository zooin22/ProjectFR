class_name RunState

enum RunStatus {
	ACTIVE,
	OBJECTIVE_COMPLETED,
	ESCAPED,
	FAILED,
	TIMED_OUT,
}

enum ObjectiveState {
	HIDDEN,
	REVEALED,
	IN_PROGRESS,
	COMPLETED,
}
