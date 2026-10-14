namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pay run request is in its life cycle.</summary>
public enum LegacyPayRunRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A legacy pay run request. Its code is unique within a tenant.</summary>
public sealed class LegacyPayRunRequest(string code, string title, string channel, double budget, string skill, int attempts, int tenureMonths, int updatedAt, LegacyPayRunRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public string Skill { get; set; } = skill;
    public int Attempts { get; set; } = attempts;
    public int TenureMonths { get; set; } = tenureMonths;
    public int UpdatedAt { get; set; } = updatedAt;
    public LegacyPayRunRequestStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pay run request in status draft.</summary>
    public static LegacyPayRunRequest NewLegacyPayRunRequest(string code, string title)
    {
        return new LegacyPayRunRequest(code, title, "", 0.0, "", 0, 0, 0, LegacyPayRunRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPayRunRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPayRunRequestStatus next)
    {
        switch (Status)
        {
            case LegacyPayRunRequestStatus.Draft:
                switch (next)
                {
                    case LegacyPayRunRequestStatus.Active:
                    case LegacyPayRunRequestStatus.Closed:
                        return true;
                }
                return false;
            case LegacyPayRunRequestStatus.Pending:
                return next == LegacyPayRunRequestStatus.Active || next == LegacyPayRunRequestStatus.Suspended;
            case LegacyPayRunRequestStatus.Active:
                switch (next)
                {
                    case LegacyPayRunRequestStatus.Suspended:
                        return true;
                }
                return false;
            case LegacyPayRunRequestStatus.Suspended:
                switch (next)
                {
                    case LegacyPayRunRequestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPayRunRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pay run request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetTotal()
    {
        return Math.Round((double)UpdatedAt * (double)TenureMonths, 2);
    }

    public LegacyPayRunRequest Copy()
    {
        return new LegacyPayRunRequest(
            Code,
            Title,
            Channel,
            Budget,
            Skill,
            Attempts,
            TenureMonths,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
