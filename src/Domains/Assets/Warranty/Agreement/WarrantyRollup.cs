namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty rollup is in its life cycle.</summary>
public enum WarrantyRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One warranty rollup, with what the planners need to act on it.</summary>
public sealed class WarrantyRollup(string code, string name, int attempts, string externalRef, double rating, WarrantyRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string ExternalRef { get; set; } = externalRef;
    public double Rating { get; set; } = rating;
    public WarrantyRollupStatus Status { get; set; } = status;

    /// <summary>Starts a warranty rollup in status created.</summary>
    public static WarrantyRollup NewWarrantyRollup(string code, string name)
    {
        return new WarrantyRollup(code, name, 0, "", 0.0, WarrantyRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyRollupStatus next)
    {
        switch (Status)
        {
            case WarrantyRollupStatus.Created:
                switch (next)
                {
                    case WarrantyRollupStatus.Retired:
                        return true;
                }
                return false;
            case WarrantyRollupStatus.Validated:
                return next == WarrantyRollupStatus.Published || next == WarrantyRollupStatus.Retired;
            case WarrantyRollupStatus.Published:
                return next == WarrantyRollupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WarrantyRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Rating + (double)Attempts, 2);
    }

    public WarrantyRollup Copy()
    {
        return new WarrantyRollup(Code, Name, Attempts, ExternalRef, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
