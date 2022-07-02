using System;
using System.Text.RegularExpressions;

namespace Server.AutoUpdater;

public class Tag
{
    public readonly int[] versionInfo = new int[3];
    public Tag(string tagName)
    {
        var groups = Regex.Match(tagName, @"(\d+)\.(\d+)\.(\d+)").Groups;

        for (var i = 1; i < groups.Count; i++)
        {
            versionInfo[i-1] = Convert.ToInt32(groups[i].Value);
        }
    }

    public static bool operator >(Tag left, Tag right)
    {
        for (var i = 0; i < 3; i++)
        {
            if (left.versionInfo[i] > right.versionInfo[i])
            {
                return true;
            }
        }

        return false;
    }
    
    public static bool operator ==(Tag left, Tag right)
    {
        if (left is null)
        {
            return right is null;
        }
        return left.Equals(right);
    }
    
    public static bool operator !=(Tag left, Tag right)
    {
        if (left is null)
        {
            return right is not null;
        }
        return !left.Equals(right);
    }
    
    public override bool Equals(object obj) => this.Equals(obj as Tag);
    public override int GetHashCode() => (versionInfo[0], versionInfo[1], versionInfo[2]).GetHashCode();
    
    private bool Equals(Tag right)
    {
        return this.versionInfo[0] == right.versionInfo[0] &&
               this.versionInfo[1] == right.versionInfo[1] &&
               this.versionInfo[2] == right.versionInfo[2];
    }
    
    public static bool operator <(Tag left, Tag right)
    {
        for (var i = 0; i < 3; i++)
        {
            if (left.versionInfo[i] < right.versionInfo[i])
            {
                return true;
            }
        }
        return false;
    }

    public override string ToString()
    {
        return $"{versionInfo[0]}.{versionInfo[1]}.{versionInfo[2]}";
    }
}