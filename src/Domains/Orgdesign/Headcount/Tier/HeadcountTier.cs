namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount tier is in its life cycle.</summary>
public enum HeadcountTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One headcount tier, with what the planners need to act on it.</summary>
public sealed class HeadcountTier(string code, string label, int capacity, double fte, string costCenter, string contactEmail, List<string> tags, HeadcountTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public double Fte { get; set; } = fte;
    public string CostCenter { get; set; } = costCenter;
    public string ContactEmail { get; set; } = contactEmail;
    public List<string> Tags { get; set; } = tags;
    public HeadcountTierStatus Status { get; set; } = status;

    /// <summary>Starts a headcount tier in status created.</summary>
    public static HeadcountTier NewHeadcountTier(string code, string label)
    {
        return new HeadcountTier(code, label, 0, 0.0, "", "", new List<string>(), HeadcountTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountTierStatus next)
    {
        switch (Status)
        {
            case HeadcountTierStatus.Created:
                switch (next)
                {
                    case HeadcountTierStatus.Published:
                    case HeadcountTierStatus.Retired:
                        return true;
                }
                return false;
            case HeadcountTierStatus.Validated:
                return next == HeadcountTierStatus.Retired;
            case HeadcountTierStatus.Published:
                return next == HeadcountTierStatus.Retired;
        }
        return false;
    }

    public void MoveTo(HeadcountTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetTotal()
    {
        return Math.Round(Fte + (double)Capacity, 2);
    }

    public HeadcountTier Copy()
    {
        return new HeadcountTier(Code, Label, Capacity, Fte, CostCenter, ContactEmail, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
