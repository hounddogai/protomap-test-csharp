namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default span claim is in its life cycle.</summary>
public enum DefaultSpanClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a default span claim and its current status.</summary>
public sealed class DefaultSpanClaim(string code, string label, int expiresAt, string unit, double budget, double minimum, Dictionary<string, string> attributes, string salaryBand, int sequence, int quantity, string locale, DefaultSpanClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Unit { get; set; } = unit;
    public double Budget { get; set; } = budget;
    public double Minimum { get; set; } = minimum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string SalaryBand { get; set; } = salaryBand;
    public int Sequence { get; set; } = sequence;
    public int Quantity { get; set; } = quantity;
    public string Locale { get; set; } = locale;
    public DefaultSpanClaimStatus Status { get; set; } = status;

    /// <summary>Starts a default span claim in status draft.</summary>
    public static DefaultSpanClaim NewDefaultSpanClaim(string code, string label)
    {
        return new DefaultSpanClaim(
            code,
            label,
            0,
            "",
            0.0,
            0.0,
            new Dictionary<string, string>(),
            "",
            0,
            0,
            "",
            DefaultSpanClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultSpanClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultSpanClaimStatus next)
    {
        switch (Status)
        {
            case DefaultSpanClaimStatus.Draft:
                return next == DefaultSpanClaimStatus.Active || next == DefaultSpanClaimStatus.Closed;
            case DefaultSpanClaimStatus.Pending:
                switch (next)
                {
                    case DefaultSpanClaimStatus.Active:
                        return true;
                }
                return false;
            case DefaultSpanClaimStatus.Active:
                switch (next)
                {
                    case DefaultSpanClaimStatus.Suspended:
                    case DefaultSpanClaimStatus.Closed:
                        return true;
                }
                return false;
            case DefaultSpanClaimStatus.Suspended:
                switch (next)
                {
                    case DefaultSpanClaimStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultSpanClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default span claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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

    public double NetVolume()
    {
        return Math.Round(Minimum - Budget, 2);
    }

    public DefaultSpanClaim Copy()
    {
        return new DefaultSpanClaim(
            Code,
            Label,
            ExpiresAt,
            Unit,
            Budget,
            Minimum,
            Attributes,
            SalaryBand,
            Sequence,
            Quantity,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
