namespace Acme.Workforce.Engagement.Recognition.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition rule is in its life cycle.</summary>
public enum RecognitionRuleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a recognition rule and its current status.</summary>
public sealed class RecognitionRule(string code, string title, int batchSize, string channel, int endsAt, int tenureMonths, string salaryBand, string badge, Dictionary<string, string> attributes, int startsAt, RecognitionRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string Channel { get; set; } = channel;
    public int EndsAt { get; set; } = endsAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public string SalaryBand { get; set; } = salaryBand;
    public string Badge { get; set; } = badge;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int StartsAt { get; set; } = startsAt;
    public RecognitionRuleStatus Status { get; set; } = status;

    /// <summary>Starts a recognition rule in status requested.</summary>
    public static RecognitionRule NewRecognitionRule(string code, string title)
    {
        return new RecognitionRule(
            code,
            title,
            0,
            "",
            0,
            0,
            "",
            "",
            new Dictionary<string, string>(),
            0,
            RecognitionRuleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionRuleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionRuleStatus next)
    {
        switch (Status)
        {
            case RecognitionRuleStatus.Requested:
                switch (next)
                {
                    case RecognitionRuleStatus.Revoked:
                    case RecognitionRuleStatus.Expired:
                        return true;
                }
                return false;
            case RecognitionRuleStatus.Granted:
                return next == RecognitionRuleStatus.Expired;
            case RecognitionRuleStatus.Revoked:
                switch (next)
                {
                    case RecognitionRuleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)BatchSize - (double)EndsAt, 2);
    }

    public RecognitionRule Copy()
    {
        return new RecognitionRule(
            Code,
            Title,
            BatchSize,
            Channel,
            EndsAt,
            TenureMonths,
            SalaryBand,
            Badge,
            Attributes,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
