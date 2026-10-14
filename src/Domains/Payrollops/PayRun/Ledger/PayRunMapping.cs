namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run mapping is in its life cycle.</summary>
public enum PayRunMappingStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pay run mapping and its current status.</summary>
public sealed class PayRunMapping(string code, string name, double minimum, double ceiling, Dictionary<string, string> attributes, bool approved, string summary, string phone, int graceDays, int leaveDays, string ownerId, PayRunMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public double Ceiling { get; set; } = ceiling;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Approved { get; set; } = approved;
    public string Summary { get; set; } = summary;
    public string Phone { get; set; } = phone;
    public int GraceDays { get; set; } = graceDays;
    public int LeaveDays { get; set; } = leaveDays;
    public string OwnerId { get; set; } = ownerId;
    public PayRunMappingStatus Status { get; set; } = status;

    /// <summary>Starts a pay run mapping in status created.</summary>
    public static PayRunMapping NewPayRunMapping(string code, string name)
    {
        return new PayRunMapping(
            code,
            name,
            0.0,
            0.0,
            new Dictionary<string, string>(),
            false,
            "",
            "",
            0,
            0,
            "",
            PayRunMappingStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunMappingStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunMappingStatus next)
    {
        switch (Status)
        {
            case PayRunMappingStatus.Created:
                switch (next)
                {
                    case PayRunMappingStatus.Retired:
                        return true;
                }
                return false;
            case PayRunMappingStatus.Validated:
                switch (next)
                {
                    case PayRunMappingStatus.Published:
                        return true;
                }
                return false;
            case PayRunMappingStatus.Published:
                return next == PayRunMappingStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PayRunMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double ProjectedLoad()
    {
        return Math.Round((double)LeaveDays * Minimum, 2);
    }

    public PayRunMapping Copy()
    {
        return new PayRunMapping(
            Code,
            Name,
            Minimum,
            Ceiling,
            Attributes,
            Approved,
            Summary,
            Phone,
            GraceDays,
            LeaveDays,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
