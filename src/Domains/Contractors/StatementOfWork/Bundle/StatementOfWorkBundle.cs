namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work bundle is in its life cycle.</summary>
public enum StatementOfWorkBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A statement of work bundle as the statement of work team keeps it.</summary>
public sealed class StatementOfWorkBundle(string code, string name, int updatedAt, string channel, double budget, Dictionary<string, string> attributes, StatementOfWorkBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public StatementOfWorkBundleStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work bundle in status draft.</summary>
    public static StatementOfWorkBundle NewStatementOfWorkBundle(string code, string name)
    {
        return new StatementOfWorkBundle(
            code,
            name,
            0,
            "",
            0.0,
            new Dictionary<string, string>(),
            StatementOfWorkBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkBundleStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkBundleStatus.Draft:
                switch (next)
                {
                    case StatementOfWorkBundleStatus.Pending:
                    case StatementOfWorkBundleStatus.Closed:
                        return true;
                }
                return false;
            case StatementOfWorkBundleStatus.Pending:
                return next == StatementOfWorkBundleStatus.Suspended;
            case StatementOfWorkBundleStatus.Active:
                return next == StatementOfWorkBundleStatus.Closed;
            case StatementOfWorkBundleStatus.Suspended:
                return next == StatementOfWorkBundleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double EffectiveLoad()
    {
        return Math.Round(Budget - (double)UpdatedAt, 2);
    }

    public StatementOfWorkBundle Copy()
    {
        return new StatementOfWorkBundle(Code, Name, UpdatedAt, Channel, Budget, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
