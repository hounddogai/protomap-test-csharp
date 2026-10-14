namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary warranty rollup is in its life cycle.</summary>
public enum PrimaryWarrantyRollupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One primary warranty rollup, with what the planners need to act on it.</summary>
public sealed class PrimaryWarrantyRollup(string code, string title, int graceDays, string contactEmail, int priority, int endsAt, string costCenter, Dictionary<string, string> attributes, int updatedAt, PrimaryWarrantyRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public string ContactEmail { get; set; } = contactEmail;
    public int Priority { get; set; } = priority;
    public int EndsAt { get; set; } = endsAt;
    public string CostCenter { get; set; } = costCenter;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int UpdatedAt { get; set; } = updatedAt;
    public PrimaryWarrantyRollupStatus Status { get; set; } = status;

    /// <summary>Starts a primary warranty rollup in status open.</summary>
    public static PrimaryWarrantyRollup NewPrimaryWarrantyRollup(string code, string title)
    {
        return new PrimaryWarrantyRollup(
            code,
            title,
            0,
            "",
            0,
            0,
            "",
            new Dictionary<string, string>(),
            0,
            PrimaryWarrantyRollupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryWarrantyRollupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryWarrantyRollupStatus next)
    {
        switch (Status)
        {
            case PrimaryWarrantyRollupStatus.Open:
                switch (next)
                {
                    case PrimaryWarrantyRollupStatus.InReview:
                    case PrimaryWarrantyRollupStatus.Reopened:
                        return true;
                }
                return false;
            case PrimaryWarrantyRollupStatus.InReview:
                switch (next)
                {
                    case PrimaryWarrantyRollupStatus.Resolved:
                        return true;
                }
                return false;
            case PrimaryWarrantyRollupStatus.Resolved:
                return next == PrimaryWarrantyRollupStatus.Reopened || next == PrimaryWarrantyRollupStatus.Archived;
            case PrimaryWarrantyRollupStatus.Reopened:
                switch (next)
                {
                    case PrimaryWarrantyRollupStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryWarrantyRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary warranty rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"primary warranty rollup"} {Code}";
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
        return Math.Round((double)EndsAt + (double)UpdatedAt, 2);
    }

    public PrimaryWarrantyRollup Copy()
    {
        return new PrimaryWarrantyRollup(
            Code,
            Title,
            GraceDays,
            ContactEmail,
            Priority,
            EndsAt,
            CostCenter,
            Attributes,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
