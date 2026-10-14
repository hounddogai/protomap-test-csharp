namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive tax form score is in its life cycle.</summary>
public enum AdaptiveTaxFormScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One adaptive tax form score, with what the planners need to act on it.</summary>
public sealed class AdaptiveTaxFormScore(string code, string name, Dictionary<string, string> attributes, bool pinned, int endsAt, AdaptiveTaxFormScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Pinned { get; set; } = pinned;
    public int EndsAt { get; set; } = endsAt;
    public AdaptiveTaxFormScoreStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive tax form score in status requested.</summary>
    public static AdaptiveTaxFormScore NewAdaptiveTaxFormScore(string code, string name)
    {
        return new AdaptiveTaxFormScore(
            code,
            name,
            new Dictionary<string, string>(),
            false,
            0,
            AdaptiveTaxFormScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTaxFormScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTaxFormScoreStatus next)
    {
        switch (Status)
        {
            case AdaptiveTaxFormScoreStatus.Requested:
                switch (next)
                {
                    case AdaptiveTaxFormScoreStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveTaxFormScoreStatus.Granted:
                switch (next)
                {
                    case AdaptiveTaxFormScoreStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveTaxFormScoreStatus.Revoked:
                switch (next)
                {
                    case AdaptiveTaxFormScoreStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveTaxFormScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive tax form score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"adaptive tax form score"} {Code}";
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

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public AdaptiveTaxFormScore Copy()
    {
        return new AdaptiveTaxFormScore(Code, Name, Attributes, Pinned, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
