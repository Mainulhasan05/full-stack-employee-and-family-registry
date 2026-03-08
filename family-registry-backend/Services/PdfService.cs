using family_registry_backend.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace family_registry_backend.Services;

public class PdfService
{
    public byte[] GenerateTablePdf(List<EmployeeResponseDto> employees)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("Employee Registry — Filtered List")
                        .FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(5).Text($"Generated on: {DateTime.Now:dd MMM yyyy, hh:mm tt}")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(30);   // #
                        cols.RelativeColumn(3);    // Name
                        cols.RelativeColumn(2.5f); // NID
                        cols.RelativeColumn(2);    // Phone
                        cols.RelativeColumn(2);    // Department
                        cols.RelativeColumn(1.5f); // Salary
                    });

                    // Header
                    table.Header(header =>
                    {
                        var headerStyle = TextStyle.Default.Bold().FontColor(Colors.White).FontSize(9);

                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("#").Style(headerStyle);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Name").Style(headerStyle);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("NID").Style(headerStyle);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Phone").Style(headerStyle);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Department").Style(headerStyle);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Salary (BDT)").Style(headerStyle);
                    });

                    // Rows
                    for (int i = 0; i < employees.Count; i++)
                    {
                        var emp = employees[i];
                        var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        table.Cell().Background(bgColor).Padding(5).Text((i + 1).ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(emp.Name);
                        table.Cell().Background(bgColor).Padding(5).Text(emp.NID);
                        table.Cell().Background(bgColor).Padding(5).Text(emp.Phone);
                        table.Cell().Background(bgColor).Padding(5).Text(emp.Department);
                        table.Cell().Background(bgColor).Padding(5).AlignRight()
                            .Text($"৳{emp.BasicSalary:N0}");
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateEmployeeCvPdf(EmployeeResponseDto emp)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("Employee CV / Profile Summary")
                        .FontSize(20).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Blue.Darken3);
                });

                page.Content().PaddingTop(20).Column(col =>
                {
                    // Personal Information
                    col.Item().Text("Personal Information")
                        .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                    col.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeColumn().Column(inner =>
                        {
                            AddField(inner, "Full Name", emp.Name);
                            AddField(inner, "NID", emp.NID);
                            AddField(inner, "Phone", emp.Phone);
                        });
                        row.RelativeColumn().Column(inner =>
                        {
                            AddField(inner, "Department", emp.Department);
                            AddField(inner, "Basic Salary", $"৳{emp.BasicSalary:N0} /month");
                        });
                    });

                    col.Item().PaddingTop(20);

                    // Spouse Information
                    col.Item().Text("Spouse Information")
                        .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                    if (emp.Spouse != null)
                    {
                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeColumn().Column(inner =>
                            {
                                AddField(inner, "Spouse Name", emp.Spouse.Name);
                            });
                            row.RelativeColumn().Column(inner =>
                            {
                                AddField(inner, "Spouse NID", emp.Spouse.NID);
                            });
                        });
                    }
                    else
                    {
                        col.Item().PaddingTop(10).Text("No spouse information on record.")
                            .Italic().FontColor(Colors.Grey.Darken1);
                    }

                    col.Item().PaddingTop(20);

                    // Children Information
                    col.Item().Text("Children Information")
                        .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                    if (emp.Children.Any())
                    {
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(30);
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(1.5f);
                            });

                            var headerStyle = TextStyle.Default.Bold().FontColor(Colors.White).FontSize(10);

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("#").Style(headerStyle);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Name").Style(headerStyle);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Date of Birth").Style(headerStyle);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Age").Style(headerStyle);
                            });

                            for (int i = 0; i < emp.Children.Count; i++)
                            {
                                var child = emp.Children[i];
                                var age = DateTime.Now.Year - child.DateOfBirth.Year;
                                var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                table.Cell().Background(bgColor).Padding(5).Text((i + 1).ToString());
                                table.Cell().Background(bgColor).Padding(5).Text(child.Name);
                                table.Cell().Background(bgColor).Padding(5).Text(child.DateOfBirth.ToString("dd MMM yyyy"));
                                table.Cell().Background(bgColor).Padding(5).Text($"{age} years");
                            }
                        });
                    }
                    else
                    {
                        col.Item().PaddingTop(10).Text("No children information on record.")
                            .Italic().FontColor(Colors.Grey.Darken1);
                    }

                    // Footer line
                    col.Item().PaddingTop(30).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(5).Text($"Generated on: {DateTime.Now:dd MMM yyyy, hh:mm tt}")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void AddField(ColumnDescriptor col, string label, string value)
    {
        col.Item().PaddingBottom(8).Row(row =>
        {
            row.AutoItem().Text($"{label}: ").Bold().FontSize(10);
            row.RelativeColumn().Text(value).FontSize(10);
        });
    }
}
