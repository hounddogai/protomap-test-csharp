namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy laptop template is in its life cycle.</summary>
public enum LegacyLaptopTemplateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A legacy laptop template. Its code is unique within a tenant.</summary>
public sealed class LegacyLaptopTemplate(string code, string title, double threshold, string badge, string ownerId, int quantity, double margin, string phone, bool approved, double budget, List<string> tags, LegacyLaptopTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string Badge { get; set; } = badge;
    public string OwnerId { get; set; } = ownerId;
    public int Quantity { get; set; } = quantity;
    public double Margin { get; set; } = margin;
    public string Phone { get; set; } = phone;
    public bool Approved { get; set; } = approved;
    public double Budget { get; set; } = budget;
    public List<string> Tags { get; set; } = tags;
    public LegacyLaptopTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a legacy laptop template in status created.</summary>
    public static LegacyLaptopTemplate NewLegacyLaptopTemplate(string code, string title)
    {
        return new LegacyLaptopTemplate(
            code,
            title,
            0.0,
            "",
            "",
            0,
            0.0,
            "",
            false,
            0.0,
            new List<string>(),
            LegacyLaptopTemplateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyLaptopTemplateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyLaptopTemplateStatus next)
    {
        switch (Status)
        {
            case LegacyLaptopTemplateStatus.Created:
                switch (next)
                {
                    case LegacyLaptopTemplateStatus.Validated:
                    case LegacyLaptopTemplateStatus.Retired:
                        return true;
                }
                return false;
            case LegacyLaptopTemplateStatus.Validated:
                switch (next)
                {
                    case LegacyLaptopTemplateStatus.Published:
                    case LegacyLaptopTemplateStatus.Retired:
                        return true;
                }
                return false;
            case LegacyLaptopTemplateStatus.Published:
                return next == LegacyLaptopTemplateStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LegacyLaptopTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy laptop template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double ProjectedLoad()
    {
        return Math.Round(Budget - (double)Quantity, 2);
    }

    public LegacyLaptopTemplate Copy()
    {
        return new LegacyLaptopTemplate(
            Code,
            Title,
            Threshold,
            Badge,
            OwnerId,
            Quantity,
            Margin,
            Phone,
            Approved,
            Budget,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
