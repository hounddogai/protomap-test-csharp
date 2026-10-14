namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal tax form template is in its life cycle.</summary>
public enum SeasonalTaxFormTemplateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a seasonal tax form template and its current status.</summary>
public sealed class SeasonalTaxFormTemplate(string code, string name, Dictionary<string, string> attributes, double threshold, int attempts, string country, SeasonalTaxFormTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Threshold { get; set; } = threshold;
    public int Attempts { get; set; } = attempts;
    public string Country { get; set; } = country;
    public SeasonalTaxFormTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal tax form template in status idle.</summary>
    public static SeasonalTaxFormTemplate NewSeasonalTaxFormTemplate(string code, string name)
    {
        return new SeasonalTaxFormTemplate(
            code,
            name,
            new Dictionary<string, string>(),
            0.0,
            0,
            "",
            SeasonalTaxFormTemplateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalTaxFormTemplateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalTaxFormTemplateStatus next)
    {
        switch (Status)
        {
            case SeasonalTaxFormTemplateStatus.Idle:
                switch (next)
                {
                    case SeasonalTaxFormTemplateStatus.Warming:
                    case SeasonalTaxFormTemplateStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalTaxFormTemplateStatus.Warming:
                return next == SeasonalTaxFormTemplateStatus.Ready || next == SeasonalTaxFormTemplateStatus.Draining;
            case SeasonalTaxFormTemplateStatus.Ready:
                return next == SeasonalTaxFormTemplateStatus.Stopped;
            case SeasonalTaxFormTemplateStatus.Draining:
                return next == SeasonalTaxFormTemplateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SeasonalTaxFormTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal tax form template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double NetLoad()
    {
        return Math.Round((double)Attempts + Threshold, 2);
    }

    public SeasonalTaxFormTemplate Copy()
    {
        return new SeasonalTaxFormTemplate(Code, Name, Attributes, Threshold, Attempts, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
