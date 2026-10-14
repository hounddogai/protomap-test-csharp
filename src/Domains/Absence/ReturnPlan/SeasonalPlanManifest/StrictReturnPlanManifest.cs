namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict return plan manifest is in its life cycle.</summary>
public enum StrictReturnPlanManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a strict return plan manifest and its current status.</summary>
public sealed class StrictReturnPlanManifest(string code, string title, double rating, string label, string location, StrictReturnPlanManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public StrictReturnPlanManifestStatus Status { get; set; } = status;

    /// <summary>Starts a strict return plan manifest in status requested.</summary>
    public static StrictReturnPlanManifest NewStrictReturnPlanManifest(string code, string title)
    {
        return new StrictReturnPlanManifest(code, title, 0.0, "", "", StrictReturnPlanManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictReturnPlanManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictReturnPlanManifestStatus next)
    {
        switch (Status)
        {
            case StrictReturnPlanManifestStatus.Requested:
                return next == StrictReturnPlanManifestStatus.Granted;
            case StrictReturnPlanManifestStatus.Granted:
                switch (next)
                {
                    case StrictReturnPlanManifestStatus.Revoked:
                    case StrictReturnPlanManifestStatus.Expired:
                        return true;
                }
                return false;
            case StrictReturnPlanManifestStatus.Revoked:
                switch (next)
                {
                    case StrictReturnPlanManifestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictReturnPlanManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict return plan manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"strict return plan manifest"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public StrictReturnPlanManifest Copy()
    {
        return new StrictReturnPlanManifest(Code, Title, Rating, Label, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
