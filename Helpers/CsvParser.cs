using Microsoft.VisualBasic.FileIO;
using SmartInvestor.Models;

namespace SmartInvestor.Helpers;

public static class CsvParser
{
    public static List<Company> GetCompanies(string path)
    {
        var companies = new List<Company>();
        using var csvParser = new TextFieldParser(path);
        csvParser.CommentTokens = [ "#" ];
        csvParser.SetDelimiters([","]);
        csvParser.HasFieldsEnclosedInQuotes = true;
        csvParser.ReadLine();
        while (!csvParser.EndOfData)
        {
            var fields = csvParser.ReadFields();
            if (fields is { Length: > 7 })
            {
                companies.Add(new Company
                {
                    Symbol = fields[0],
                    Name = fields[1],
                    GicsSector = fields[2],
                    GicsSubIndustry = fields[3],
                    HeadquartersLocation = fields[4],
                    AddedDateInSp500 = DateTime.TryParse(fields[5], out var addedDate) ? addedDate : null,
                    Cik = fields[6].PadLeft(10, '0'),
                    FoundedYear = fields[7]
                });
            }
        }
        return companies;
    }
}