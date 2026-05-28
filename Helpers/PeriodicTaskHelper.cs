using CarRentWeb.Models;

namespace CarRentWeb.Helpers;

public static class PeriodicTaskHelper
{
    public static Dictionary<string, string> EmployeeDateFields => new()
    {
        { "ResEndDate",      "نهاية الإقامة" },
        { "EndLicense",      "نهاية الرخصة" },
        { "PassportEndDate", "نهاية الجواز" },
        { "CivilIdendDate",  "نهاية البطاقة المدنية" }
    };

    public static Dictionary<string, string> CarDateFields => new()
    {
        { "CarEndLicense", "نهاية رخصة السيارة" },
        { "CarEndDate",    "نهاية التأمين / الدفتر" }
    };

    public static string GetFieldLabel(string sourceType, string fieldName)
    {
        var dict = sourceType == "Car" ? CarDateFields : EmployeeDateFields;
        return dict.TryGetValue(fieldName, out var label) ? label : fieldName;
    }

    public static DateOnly? GetEmployeeDate(EmployeeInfo emp, string fieldName) => fieldName switch
    {
        "ResEndDate" => emp.ResEndDate,
        "EndLicense" => emp.EndLicense,
        "PassportEndDate" => emp.PassportEndDate,
        "CivilIdendDate" => emp.CivilIdendDate,
        _ => null
    };

    public static DateOnly? GetCarDate(CarInfo car, string fieldName) => fieldName switch
    {
        "CarEndLicense" => car.CarEndLicense,
        "CarEndDate" => car.CarEndDate,
        _ => null
    };

    public static string GetSourceLabel(string sourceType) =>
        sourceType == "Car" ? "السيارات" : "الموظفين";
}