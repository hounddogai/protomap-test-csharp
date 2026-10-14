namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global scenario request is in its life cycle.</summary>
public enum GlobalScenarioRequestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A global scenario request. Its code is unique within a tenant.</summary>
public sealed class GlobalScenarioRequest(string code, string title, int tenureMonths, bool visible, int version, string reviewer, double score, double fte, int priority, string phone, string source, GlobalScenarioRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Visible { get; set; } = visible;
    public int Version { get; set; } = version;
    public string Reviewer { get; set; } = reviewer;
    public double Score { get; set; } = score;
    public double Fte { get; set; } = fte;
    public int Priority { get; set; } = priority;
    public string Phone { get; set; } = phone;
    public string Source { get; set; } = source;
    public GlobalScenarioRequestStatus Status { get; set; } = status;

    /// <summary>Starts a global scenario request in status requested.</summary>
    public static GlobalScenarioRequest NewGlobalScenarioRequest(string code, string title)
    {
        return new GlobalScenarioRequest(
            code,
            title,
            0,
            false,
            0,
            "",
            0.0,
            0.0,
            0,
            "",
            "",
            GlobalScenarioRequestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalScenarioRequestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalScenarioRequestStatus next)
    {
        switch (Status)
        {
            case GlobalScenarioRequestStatus.Requested:
                switch (next)
                {
                    case GlobalScenarioRequestStatus.Granted:
                    case GlobalScenarioRequestStatus.Expired:
                        return true;
                }
                return false;
            case GlobalScenarioRequestStatus.Granted:
                switch (next)
                {
                    case GlobalScenarioRequestStatus.Revoked:
                    case GlobalScenarioRequestStatus.Expired:
                        return true;
                }
                return false;
            case GlobalScenarioRequestStatus.Revoked:
                switch (next)
                {
                    case GlobalScenarioRequestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalScenarioRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global scenario request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"global scenario request"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Score - (double)Priority, 2);
    }

    public GlobalScenarioRequest Copy()
    {
        return new GlobalScenarioRequest(
            Code,
            Title,
            TenureMonths,
            Visible,
            Version,
            Reviewer,
            Score,
            Fte,
            Priority,
            Phone,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
