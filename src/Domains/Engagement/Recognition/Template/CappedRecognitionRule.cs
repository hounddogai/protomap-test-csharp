namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped recognition rule is in its life cycle.</summary>
public enum CappedRecognitionRuleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One capped recognition rule, with what the planners need to act on it.</summary>
public sealed class CappedRecognitionRule(string code, string title, string phone, string category, int endsAt, string badge, CappedRecognitionRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public string Category { get; set; } = category;
    public int EndsAt { get; set; } = endsAt;
    public string Badge { get; set; } = badge;
    public CappedRecognitionRuleStatus Status { get; set; } = status;

    /// <summary>Starts a capped recognition rule in status requested.</summary>
    public static CappedRecognitionRule NewCappedRecognitionRule(string code, string title)
    {
        return new CappedRecognitionRule(code, title, "", "", 0, "", CappedRecognitionRuleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedRecognitionRuleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedRecognitionRuleStatus next)
    {
        switch (Status)
        {
            case CappedRecognitionRuleStatus.Requested:
                switch (next)
                {
                    case CappedRecognitionRuleStatus.Granted:
                    case CappedRecognitionRuleStatus.Expired:
                        return true;
                }
                return false;
            case CappedRecognitionRuleStatus.Granted:
                switch (next)
                {
                    case CappedRecognitionRuleStatus.Revoked:
                    case CappedRecognitionRuleStatus.Expired:
                        return true;
                }
                return false;
            case CappedRecognitionRuleStatus.Revoked:
                return next == CappedRecognitionRuleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CappedRecognitionRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped recognition rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"capped recognition rule"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public CappedRecognitionRule Copy()
    {
        return new CappedRecognitionRule(Code, Title, Phone, Category, EndsAt, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
