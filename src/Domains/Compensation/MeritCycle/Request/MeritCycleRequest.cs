namespace Acme.Workforce.Compensation.MeritCycle.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle request is in its life cycle.</summary>
public enum MeritCycleRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a merit cycle request and its current status.</summary>
public sealed class MeritCycleRequest(string code, string name, double rating, string notes, double ratio, string managerId, string salaryBand, double margin, MeritCycleRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public string Notes { get; set; } = notes;
    public double Ratio { get; set; } = ratio;
    public string ManagerId { get; set; } = managerId;
    public string SalaryBand { get; set; } = salaryBand;
    public double Margin { get; set; } = margin;
    public MeritCycleRequestStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle request in status created.</summary>
    public static MeritCycleRequest NewMeritCycleRequest(string code, string name)
    {
        return new MeritCycleRequest(code, name, 0.0, "", 0.0, "", "", 0.0, MeritCycleRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleRequestStatus next)
    {
        switch (Status)
        {
            case MeritCycleRequestStatus.Created:
                switch (next)
                {
                    case MeritCycleRequestStatus.Retired:
                        return true;
                }
                return false;
            case MeritCycleRequestStatus.Validated:
                switch (next)
                {
                    case MeritCycleRequestStatus.Published:
                    case MeritCycleRequestStatus.Retired:
                        return true;
                }
                return false;
            case MeritCycleRequestStatus.Published:
                return next == MeritCycleRequestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(MeritCycleRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Ratio - Rating, 2);
    }

    public MeritCycleRequest Copy()
    {
        return new MeritCycleRequest(Code, Name, Rating, Notes, Ratio, ManagerId, SalaryBand, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
