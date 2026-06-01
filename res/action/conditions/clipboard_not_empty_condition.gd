class_name ClipboardNotEmptyCondition
extends ActionCondition

func _init() -> void:
	condition_id = "clipboard_not_empty"

func check(context: ActionContext) -> bool:
	return context.clipboard_has_content
