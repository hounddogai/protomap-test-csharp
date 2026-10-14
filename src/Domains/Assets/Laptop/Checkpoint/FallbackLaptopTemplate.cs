namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback laptop template is in its life cycle.</summary>
public enum FallbackLaptopTemplateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a fallback laptop template and its current status.</summary>
public sealed class FallbackLaptopTemplate(string code, string label, string title, bool active, int startsAt, FallbackLaptopTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public int StartsAt { get; set; } = startsAt;
    public FallbackLaptopTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a fallback laptop template in status created.</summary>
    public static FallbackLaptopTemplate NewFallbackLaptopTemplate(string code, string label)
    {
        return new FallbackLaptopTemplate(code, label, "", false, 0, FallbackLaptopTemplateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackLaptopTemplateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackLaptopTemplateStatus next)
    {
        switch (Status)
        {
            case FallbackLaptopTemplateStatus.Created:
                return next == FallbackLaptopTemplateStatus.Validated;
            case FallbackLaptopTemplateStatus.Validated:
                return next == FallbackLaptopTemplateStatus.Published;
            case FallbackLaptopTemplateStatus.Published:
                return next == FallbackLaptopTemplateStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FallbackLaptopTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback laptop template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public FallbackLaptopTemplate Copy()
    {
        return new FallbackLaptopTemplate(Code, Label, Title, Active, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
