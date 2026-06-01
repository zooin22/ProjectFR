extends Node

enum ClipboardMode {
	COPY,
	CUT,
}

var _content: NodeData = null
var _mode: int = ClipboardMode.COPY

var content: NodeData:
	get: return _content

var mode: int:
	get: return _mode

var has_content: bool:
	get: return _content != null

func copy(node: NodeData) -> void:
	_content = node
	_mode = ClipboardMode.COPY

func cut(node: NodeData) -> void:
	_content = node
	_mode = ClipboardMode.CUT

func paste() -> NodeData:
	if _content == null:
		return null
	var result := _content
	if _mode == ClipboardMode.CUT:
		_content = null
	return result

func clear() -> void:
	_content = null
