namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default attestation template is in its life cycle.</summary>
public enum DefaultAttestationTemplateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One default attestation template, with what the planners need to act on it.</summary>
public sealed class DefaultAttestationTemplate(string code, string title, string currency, double hours, string channel, bool active, string name, string region, DefaultAttestationTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public double Hours { get; set; } = hours;
    public string Channel { get; set; } = channel;
    public bool Active { get; set; } = active;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public DefaultAttestationTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a default attestation template in status created.</summary>
    public static DefaultAttestationTemplate NewDefaultAttestationTemplate(string code, string title)
    {
        return new DefaultAttestationTemplate(
            code,
            title,
            "",
            0.0,
            "",
            false,
            "",
            "",
            DefaultAttestationTemplateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAttestationTemplateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAttestationTemplateStatus next)
    {
        switch (Status)
        {
            case DefaultAttestationTemplateStatus.Created:
                switch (next)
                {
                    case DefaultAttestationTemplateStatus.Validated:
                    case DefaultAttestationTemplateStatus.Retired:
                        return true;
                }
                return false;
            case DefaultAttestationTemplateStatus.Validated:
                switch (next)
                {
                    case DefaultAttestationTemplateStatus.Retired:
                        return true;
                }
                return false;
            case DefaultAttestationTemplateStatus.Published:
                return next == DefaultAttestationTemplateStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DefaultAttestationTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default attestation template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Hours;
    }

    public DefaultAttestationTemplate Copy()
    {
        return new DefaultAttestationTemplate(Code, Title, Currency, Hours, Channel, Active, Name, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
