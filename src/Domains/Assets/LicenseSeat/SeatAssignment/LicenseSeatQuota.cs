namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat quota is in its life cycle.</summary>
public enum LicenseSeatQuotaStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a license seat quota and its current status.</summary>
public sealed class LicenseSeatQuota(string code, string name, string grade, double margin, int graceDays, double budget, string team, double score, double rate, LicenseSeatQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public double Margin { get; set; } = margin;
    public int GraceDays { get; set; } = graceDays;
    public double Budget { get; set; } = budget;
    public string Team { get; set; } = team;
    public double Score { get; set; } = score;
    public double Rate { get; set; } = rate;
    public LicenseSeatQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a license seat quota in status draft.</summary>
    public static LicenseSeatQuota NewLicenseSeatQuota(string code, string name)
    {
        return new LicenseSeatQuota(code, name, "", 0.0, 0, 0.0, "", 0.0, 0.0, LicenseSeatQuotaStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatQuotaStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatQuotaStatus next)
    {
        switch (Status)
        {
            case LicenseSeatQuotaStatus.Draft:
                return next == LicenseSeatQuotaStatus.Suspended;
            case LicenseSeatQuotaStatus.Pending:
                switch (next)
                {
                    case LicenseSeatQuotaStatus.Active:
                    case LicenseSeatQuotaStatus.Closed:
                        return true;
                }
                return false;
            case LicenseSeatQuotaStatus.Active:
                switch (next)
                {
                    case LicenseSeatQuotaStatus.Suspended:
                    case LicenseSeatQuotaStatus.Closed:
                        return true;
                }
                return false;
            case LicenseSeatQuotaStatus.Suspended:
                switch (next)
                {
                    case LicenseSeatQuotaStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"license seat quota"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Score + Rate, 2);
    }

    public LicenseSeatQuota Copy()
    {
        return new LicenseSeatQuota(Code, Name, Grade, Margin, GraceDays, Budget, Team, Score, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
