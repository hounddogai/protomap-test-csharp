namespace Acme.Workforce.Assets.Warranty.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal warranty target is in its life cycle.</summary>
public enum SeasonalWarrantyTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A seasonal warranty target. Its code is unique within a tenant.</summary>
public sealed class SeasonalWarrantyTarget(string code, string title, double floorValue, int attempts, string country, string externalRef, string jobCode, string notes, string location, SeasonalWarrantyTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public int Attempts { get; set; } = attempts;
    public string Country { get; set; } = country;
    public string ExternalRef { get; set; } = externalRef;
    public string JobCode { get; set; } = jobCode;
    public string Notes { get; set; } = notes;
    public string Location { get; set; } = location;
    public SeasonalWarrantyTargetStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal warranty target in status created.</summary>
    public static SeasonalWarrantyTarget NewSeasonalWarrantyTarget(string code, string title)
    {
        return new SeasonalWarrantyTarget(
            code,
            title,
            0.0,
            0,
            "",
            "",
            "",
            "",
            "",
            SeasonalWarrantyTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalWarrantyTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalWarrantyTargetStatus next)
    {
        switch (Status)
        {
            case SeasonalWarrantyTargetStatus.Created:
                switch (next)
                {
                    case SeasonalWarrantyTargetStatus.Validated:
                        return true;
                }
                return false;
            case SeasonalWarrantyTargetStatus.Validated:
                switch (next)
                {
                    case SeasonalWarrantyTargetStatus.Published:
                    case SeasonalWarrantyTargetStatus.Retired:
                        return true;
                }
                return false;
            case SeasonalWarrantyTargetStatus.Published:
                switch (next)
                {
                    case SeasonalWarrantyTargetStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalWarrantyTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal warranty target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal warranty target"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Attempts - FloorValue, 2);
    }

    public SeasonalWarrantyTarget Copy()
    {
        return new SeasonalWarrantyTarget(
            Code,
            Title,
            FloorValue,
            Attempts,
            Country,
            ExternalRef,
            JobCode,
            Notes,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
