namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered cost model entry is in its life cycle.</summary>
public enum TieredCostModelEntryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A tiered cost model entry. Its code is unique within a tenant.</summary>
public sealed class TieredCostModelEntry(string code, string name, string region, string title, double margin, string timezone, double discount, double budget, int expiresAt, int endsAt, TieredCostModelEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public string Timezone { get; set; } = timezone;
    public double Discount { get; set; } = discount;
    public double Budget { get; set; } = budget;
    public int ExpiresAt { get; set; } = expiresAt;
    public int EndsAt { get; set; } = endsAt;
    public TieredCostModelEntryStatus Status { get; set; } = status;

    /// <summary>Starts a tiered cost model entry in status created.</summary>
    public static TieredCostModelEntry NewTieredCostModelEntry(string code, string name)
    {
        return new TieredCostModelEntry(
            code,
            name,
            "",
            "",
            0.0,
            "",
            0.0,
            0.0,
            0,
            0,
            TieredCostModelEntryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredCostModelEntryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredCostModelEntryStatus next)
    {
        switch (Status)
        {
            case TieredCostModelEntryStatus.Created:
                return next == TieredCostModelEntryStatus.Published;
            case TieredCostModelEntryStatus.Validated:
                switch (next)
                {
                    case TieredCostModelEntryStatus.Published:
                    case TieredCostModelEntryStatus.Retired:
                        return true;
                }
                return false;
            case TieredCostModelEntryStatus.Published:
                return next == TieredCostModelEntryStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TieredCostModelEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered cost model entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedValue()
    {
        return Math.Round(Margin + (double)ExpiresAt, 2);
    }

    public TieredCostModelEntry Copy()
    {
        return new TieredCostModelEntry(
            Code,
            Name,
            Region,
            Title,
            Margin,
            Timezone,
            Discount,
            Budget,
            ExpiresAt,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
