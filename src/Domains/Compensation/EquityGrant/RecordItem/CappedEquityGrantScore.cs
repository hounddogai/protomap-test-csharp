namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped equity grant score is in its life cycle.</summary>
public enum CappedEquityGrantScoreStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a capped equity grant score and its current status.</summary>
public sealed class CappedEquityGrantScore(string code, string name, int attempts, int revision, Dictionary<string, string> attributes, string jobCode, string country, CappedEquityGrantScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public int Revision { get; set; } = revision;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string JobCode { get; set; } = jobCode;
    public string Country { get; set; } = country;
    public CappedEquityGrantScoreStatus Status { get; set; } = status;

    /// <summary>Starts a capped equity grant score in status planned.</summary>
    public static CappedEquityGrantScore NewCappedEquityGrantScore(string code, string name)
    {
        return new CappedEquityGrantScore(
            code,
            name,
            0,
            0,
            new Dictionary<string, string>(),
            "",
            "",
            CappedEquityGrantScoreStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedEquityGrantScoreStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedEquityGrantScoreStatus next)
    {
        switch (Status)
        {
            case CappedEquityGrantScoreStatus.Planned:
                switch (next)
                {
                    case CappedEquityGrantScoreStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedEquityGrantScoreStatus.Scheduled:
                switch (next)
                {
                    case CappedEquityGrantScoreStatus.Completed:
                        return true;
                }
                return false;
            case CappedEquityGrantScoreStatus.InProgress:
                switch (next)
                {
                    case CappedEquityGrantScoreStatus.Completed:
                    case CappedEquityGrantScoreStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedEquityGrantScoreStatus.Completed:
                return next == CappedEquityGrantScoreStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CappedEquityGrantScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped equity grant score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"capped equity grant score"} {Code}";
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

    public double AdjustedVolume()
    {
        return Math.Round((double)Attempts * (double)Revision, 2);
    }

    public CappedEquityGrantScore Copy()
    {
        return new CappedEquityGrantScore(Code, Name, Attempts, Revision, Attributes, JobCode, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
