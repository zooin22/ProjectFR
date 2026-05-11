namespace ProjectFR.Action;

public class ActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int DamageDealt { get; set; }
    public Dictionary<string, object> Data { get; set; }

    public ActionResult(bool success, string message = "", int damage = 0)
    {
        Success = success;
        Message = message;
        DamageDealt = damage;
        Data = new();
    }
}
