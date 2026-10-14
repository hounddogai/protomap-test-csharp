namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual pay band template is in its life cycle.</summary>
public enum ManualPayBandTemplateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual pay band template. Its code is unique within a tenant.</summary>
public sealed class ManualPayBandTemplate(string code, string name, int startsAt, int sequence, int quantity, double maximum, Dictionary<string, string> attributes, ManualPayBandTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public int Sequence { get; set; } = sequence;
    public int Quantity { get; set; } = quantity;
    public double Maximum { get; set; } = maximum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public ManualPayBandTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a manual pay band template in status created.</summary>
    public static ManualPayBandTemplate NewManualPayBandTemplate(string code, string name)
    {
        return new ManualPayBandTemplate(
            code,
            name,
            0,
            0,
            0,
            0.0,
            new Dictionary<string, string>(),
            ManualPayBandTemplateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPayBandTemplateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPayBandTemplateStatus next)
    {
        switch (Status)
        {
            case ManualPayBandTemplateStatus.Created:
                switch (next)
                {
                    case ManualPayBandTemplateStatus.Validated:
                    case ManualPayBandTemplateStatus.Retired:
                        return true;
                }
                return false;
            case ManualPayBandTemplateStatus.Validated:
                switch (next)
                {
                    case ManualPayBandTemplateStatus.Published:
                    case ManualPayBandTemplateStatus.Retired:
                        return true;
                }
                return false;
            case ManualPayBandTemplateStatus.Published:
                return next == ManualPayBandTemplateStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ManualPayBandTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual pay band template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"manual pay band template"} {Code}";
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
        return Math.Round(Maximum - (double)Sequence, 2);
    }

    public ManualPayBandTemplate Copy()
    {
        return new ManualPayBandTemplate(Code, Name, StartsAt, Sequence, Quantity, Maximum, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
