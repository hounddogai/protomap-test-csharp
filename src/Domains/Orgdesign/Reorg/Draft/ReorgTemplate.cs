namespace Acme.Workforce.Orgdesign.Reorg.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg template is in its life cycle.</summary>
public enum ReorgTemplateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a reorg template and its current status.</summary>
public sealed class ReorgTemplate(string code, string name, double rate, int retries, string label, string grade, double fte, string locale, int capacity, int revision, ReorgTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public int Retries { get; set; } = retries;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public double Fte { get; set; } = fte;
    public string Locale { get; set; } = locale;
    public int Capacity { get; set; } = capacity;
    public int Revision { get; set; } = revision;
    public ReorgTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a reorg template in status created.</summary>
    public static ReorgTemplate NewReorgTemplate(string code, string name)
    {
        return new ReorgTemplate(code, name, 0.0, 0, "", "", 0.0, "", 0, 0, ReorgTemplateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgTemplateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgTemplateStatus next)
    {
        switch (Status)
        {
            case ReorgTemplateStatus.Created:
                switch (next)
                {
                    case ReorgTemplateStatus.Validated:
                        return true;
                }
                return false;
            case ReorgTemplateStatus.Validated:
                switch (next)
                {
                    case ReorgTemplateStatus.Published:
                    case ReorgTemplateStatus.Retired:
                        return true;
                }
                return false;
            case ReorgTemplateStatus.Published:
                return next == ReorgTemplateStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ReorgTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"reorg template"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Rate * Fte, 2);
    }

    public ReorgTemplate Copy()
    {
        return new ReorgTemplate(Code, Name, Rate, Retries, Label, Grade, Fte, Locale, Capacity, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
