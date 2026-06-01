class_name ActionBase

var action_id: String = ""
var display_name: String = ""
var ap_cost: int = 0
var scope: int = TargetType.SINGLE
var conditions: Array = []

func can_execute(context: ActionContext) -> bool:
	for c in conditions:
		if not c.check(context):
			return false
	return true

func execute(_context: ActionContext) -> ActionResult:
	push_error("ActionBase.execute() must be overridden in subclass: " + action_id)
	return ActionResult.new(false, "Not implemented")
