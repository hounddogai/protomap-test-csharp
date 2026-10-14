namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge agreement is in its life cycle.</summary>
public enum AccessBadgeAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One access badge agreement, with what the planners need to act on it.</summary>
public sealed class AccessBadgeAgreement(string code, string name, int retries, string managerId, double margin, double floorValue, int updatedAt, int tenureMonths, Dictionary<string, string> attributes, double ratio, AccessBadgeAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string ManagerId { get; set; } = managerId;
    public double Margin { get; set; } = margin;
    public double FloorValue { get; set; } = floorValue;
    public int UpdatedAt { get; set; } = updatedAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Ratio { get; set; } = ratio;
    public AccessBadgeAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a access badge agreement in status created.</summary>
    public static AccessBadgeAgreement NewAccessBadgeAgreement(string code, string name)
    {
        return new AccessBadgeAgreement(
            code,
            name,
            0,
            "",
            0.0,
            0.0,
            0,
            0,
            new Dictionary<string, string>(),
            0.0,
            AccessBadgeAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeAgreementStatus next)
    {
        switch (Status)
        {
            case AccessBadgeAgreementStatus.Created:
                return next == AccessBadgeAgreementStatus.Published;
            case AccessBadgeAgreementStatus.Validated:
                return next == AccessBadgeAgreementStatus.Retired;
            case AccessBadgeAgreementStatus.Published:
                switch (next)
                {
                    case AccessBadgeAgreementStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

    public double EffectiveValue()
    {
        return Math.Round((double)Retries - (double)TenureMonths, 2);
    }

    public AccessBadgeAgreement Copy()
    {
        return new AccessBadgeAgreement(
            Code,
            Name,
            Retries,
            ManagerId,
            Margin,
            FloorValue,
            UpdatedAt,
            TenureMonths,
            Attributes,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
