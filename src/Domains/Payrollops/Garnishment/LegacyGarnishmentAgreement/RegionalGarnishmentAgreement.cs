namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional garnishment agreement is in its life cycle.</summary>
public enum RegionalGarnishmentAgreementStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a regional garnishment agreement and its current status.</summary>
public sealed class RegionalGarnishmentAgreement(string code, string label, double maximum, string badge, string grade, int batchSize, bool pinned, Dictionary<string, string> attributes, int quantity, bool approved, RegionalGarnishmentAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public string Badge { get; set; } = badge;
    public string Grade { get; set; } = grade;
    public int BatchSize { get; set; } = batchSize;
    public bool Pinned { get; set; } = pinned;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Quantity { get; set; } = quantity;
    public bool Approved { get; set; } = approved;
    public RegionalGarnishmentAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a regional garnishment agreement in status proposed.</summary>
    public static RegionalGarnishmentAgreement NewRegionalGarnishmentAgreement(string code, string label)
    {
        return new RegionalGarnishmentAgreement(
            code,
            label,
            0.0,
            "",
            "",
            0,
            false,
            new Dictionary<string, string>(),
            0,
            false,
            RegionalGarnishmentAgreementStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalGarnishmentAgreementStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalGarnishmentAgreementStatus next)
    {
        switch (Status)
        {
            case RegionalGarnishmentAgreementStatus.Proposed:
                switch (next)
                {
                    case RegionalGarnishmentAgreementStatus.Approved:
                        return true;
                }
                return false;
            case RegionalGarnishmentAgreementStatus.Approved:
                return next == RegionalGarnishmentAgreementStatus.Rejected;
            case RegionalGarnishmentAgreementStatus.Rejected:
                switch (next)
                {
                    case RegionalGarnishmentAgreementStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalGarnishmentAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional garnishment agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"regional garnishment agreement"} {Code}";
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

    public double ProjectedExposure()
    {
        return Math.Round((double)BatchSize * (double)Quantity, 2);
    }

    public RegionalGarnishmentAgreement Copy()
    {
        return new RegionalGarnishmentAgreement(
            Code,
            Label,
            Maximum,
            Badge,
            Grade,
            BatchSize,
            Pinned,
            Attributes,
            Quantity,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
