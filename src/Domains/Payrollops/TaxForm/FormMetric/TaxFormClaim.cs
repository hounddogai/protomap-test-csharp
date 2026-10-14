namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form claim is in its life cycle.</summary>
public enum TaxFormClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tax form claim. Its code is unique within a tenant.</summary>
public sealed class TaxFormClaim(string code, string name, string reviewer, int limit, Dictionary<string, string> attributes, double fte, double ceiling, bool approved, string label, List<string> tags, string country, TaxFormClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public int Limit { get; set; } = limit;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Fte { get; set; } = fte;
    public double Ceiling { get; set; } = ceiling;
    public bool Approved { get; set; } = approved;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public string Country { get; set; } = country;
    public TaxFormClaimStatus Status { get; set; } = status;

    /// <summary>Starts a tax form claim in status draft.</summary>
    public static TaxFormClaim NewTaxFormClaim(string code, string name)
    {
        return new TaxFormClaim(
            code,
            name,
            "",
            0,
            new Dictionary<string, string>(),
            0.0,
            0.0,
            false,
            "",
            new List<string>(),
            "",
            TaxFormClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormClaimStatus next)
    {
        switch (Status)
        {
            case TaxFormClaimStatus.Draft:
                switch (next)
                {
                    case TaxFormClaimStatus.Pending:
                    case TaxFormClaimStatus.Closed:
                        return true;
                }
                return false;
            case TaxFormClaimStatus.Pending:
                switch (next)
                {
                    case TaxFormClaimStatus.Suspended:
                        return true;
                }
                return false;
            case TaxFormClaimStatus.Active:
                return next == TaxFormClaimStatus.Suspended;
            case TaxFormClaimStatus.Suspended:
                return next == TaxFormClaimStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TaxFormClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"tax form claim"} {Code}";
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

    public double WeightedTotal()
    {
        return Math.Round((double)Limit + Fte, 2);
    }

    public TaxFormClaim Copy()
    {
        return new TaxFormClaim(
            Code,
            Name,
            Reviewer,
            Limit,
            Attributes,
            Fte,
            Ceiling,
            Approved,
            Label,
            Tags,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
