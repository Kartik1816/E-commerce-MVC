using System.Text.Json;
using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Demo.Web.Controllers;

public class OrdersController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/Orders/";

    public OrdersController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<IActionResult> Index()
    {
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }
        int userId = JwtService.GetUserIdFromJwtToken(token);
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "get-user-orders/" + userId);
        string jsonString = await response.Content.ReadAsStringAsync();
        ResponseModel? responseModel = JsonConvert.DeserializeObject<ResponseModel>(jsonString);
        UserOrders? userOrders = JsonConvert.DeserializeObject<UserOrders>(responseModel?.Data.ToString() ?? string.Empty);

        return View(userOrders);
    }

    [HttpPost]
    public async Task<IActionResult> SaveCustomerReview(CustomerReviewModel customerReviewModel)
    {

        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }
        int userId = JwtService.GetUserIdFromJwtToken(token);
        customerReviewModel.UserId = userId;

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "reviews", customerReviewModel);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = response.Content.ReadAsStringAsync().Result;
            ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(responseContent);
            if (responseData != null)
            {
                string? message = responseData.Message;
                bool success = responseData.IsSuccess;
                return new JsonResult(new { success = success, message = message });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Invalid response from server." });
            }
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while processing the request.");
        }
    }
    
    public async Task<IActionResult> GenerateInvoice(int orderId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + orderId);
        string jsonString = await response.Content.ReadAsStringAsync();
        ResponseModel? responseModel = JsonConvert.DeserializeObject<ResponseModel>(jsonString);
        OrderDetailsViewModel? order = JsonConvert.DeserializeObject<OrderDetailsViewModel>(responseModel?.Data.ToString() ?? string.Empty);

        if (order == null)
            return NotFound();

        using (var stream = new MemoryStream())
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 10, XFontStyle.Regular);
            var boldFont = new XFont("Verdana", 10, XFontStyle.Bold);
            int y = 40;

            // Title
            gfx.DrawString("INVOICE", new XFont("Verdana", 16, XFontStyle.Bold), XBrushes.Black,
                new XRect(0, y, page.Width, 40), XStringFormats.TopCenter);
                
            y += 80;

            // Order Info
          // ====== ORDER INFO TABLE ======
            int orderTableX = 40;
            int orderTableY = y-18;
            int labelWidth = 100;
            int valueWidth = 100;
            int rowHeight = 30;

            // Order Number row
            gfx.DrawRectangle(XPens.Black, orderTableX, orderTableY, labelWidth + valueWidth, rowHeight );
            gfx.DrawRectangle(XPens.Black, orderTableX, orderTableY, labelWidth, rowHeight );
            gfx.DrawString("Order Number:", boldFont, XBrushes.Black, orderTableX + 5, orderTableY + 15);
            gfx.DrawString($"{order.CreatedAt:yyMMddHHmm}{order.OrderId}", font, XBrushes.Black, orderTableX + labelWidth + 5, orderTableY + 15);
                y+= rowHeight + 20; // Move Y forward after table
            // Order Date row
            gfx.DrawRectangle(XPens.Black, orderTableX, orderTableY + rowHeight, labelWidth + valueWidth, rowHeight );
            gfx.DrawRectangle(XPens.Black, orderTableX, orderTableY + rowHeight, labelWidth, rowHeight );
            gfx.DrawString("Order Date:", boldFont, XBrushes.Black, orderTableX + 5, orderTableY + rowHeight + 15);
            gfx.DrawString($"{order.CreatedAt:dd MMM yyyy}", font, XBrushes.Black, orderTableX + labelWidth + 5, orderTableY + rowHeight + 15);

            y +=  rowHeight + 20; // Move Y forward after table


            int customerTableX = 320;
            int customerTableY = 100;
             labelWidth = 80;
             valueWidth = 160;
             rowHeight = 20;


            gfx.DrawRectangle(XPens.Black, customerTableX, customerTableY, labelWidth + valueWidth, rowHeight);
            gfx.DrawRectangle(XPens.Black, customerTableX, customerTableY, labelWidth, rowHeight);
            gfx.DrawString("Name:", boldFont, XBrushes.Black, customerTableX + 5, customerTableY + 15);
            gfx.DrawString(order.CustomerName, font, XBrushes.Black, customerTableX + labelWidth + 5, customerTableY + 15);

            // Email row
            gfx.DrawRectangle(XPens.Black, customerTableX, customerTableY + rowHeight, labelWidth + valueWidth, rowHeight);
            gfx.DrawRectangle(XPens.Black, customerTableX, customerTableY + rowHeight, labelWidth, rowHeight);
            gfx.DrawString("Email:", boldFont, XBrushes.Black, customerTableX + 5, customerTableY + rowHeight + 15);
            gfx.DrawString(order.CustomerEmail, font, XBrushes.Black, customerTableX + labelWidth + 5, customerTableY + rowHeight + 15);

            // Address row
            string addressLabel = "Address:";
            string addressText = order.CustomerAddress;

            // Max width for value column
            double maxAddressWidth = valueWidth - 10;

            // Measure the string
            var addressSize = gfx.MeasureString(addressText, font);

            // Estimate how many lines needed (basic wrap logic)
            int addressLineCount = (int)Math.Ceiling(addressSize.Width / maxAddressWidth);
            int adjustedRowHeight = Math.Max(rowHeight, rowHeight * addressLineCount);

            // Draw full row box
            gfx.DrawRectangle(XPens.Black, customerTableX, customerTableY + 2 * rowHeight, labelWidth + valueWidth, adjustedRowHeight);

            // Draw label cell box
            gfx.DrawRectangle(XPens.Black, customerTableX, customerTableY + 2 * rowHeight, labelWidth, adjustedRowHeight);

            // Draw label
            gfx.DrawString(addressLabel, boldFont, XBrushes.Black, customerTableX + 5, customerTableY + 2 * rowHeight + 15);

            // Draw wrapped address manually
            string[] addressLines = WrapText(addressText, font, gfx, maxAddressWidth);
            for (int i = 0; i < addressLines.Length; i++)
            {
                gfx.DrawString(addressLines[i], font, XBrushes.Black, customerTableX + labelWidth + 5,
                            customerTableY + 2 * rowHeight + 15 + (i * rowHeight));

            }
           
           y += adjustedRowHeight - 35; // Move Y forward after customer info

            // Column positions
            int startX = 40;
            int colProductCode = startX;
            int colDescription = colProductCode + 60;
            int colQty = colDescription + 200;
            int colUnit = colQty + 50;
            int colNet = colUnit + 60;
            int colDiscount = colNet + 60;

            
            int tableWidth = colDiscount + 90 - startX;

            // Table Header
            gfx.DrawRectangle(XPens.Black, startX, y, tableWidth, rowHeight);
            gfx.DrawString("Product Code", boldFont, XBrushes.Black, colProductCode + 2, y + 15);
            gfx.DrawString("Description", boldFont, XBrushes.Black, colDescription + 20, y + 15);
            gfx.DrawString("Qty", boldFont, XBrushes.Black, colQty + 2, y + 15);
            gfx.DrawString("Unit", boldFont, XBrushes.Black, colUnit + 2, y + 15);
            gfx.DrawString("Net ₹", boldFont, XBrushes.Black, colNet + 2, y + 15);
            gfx.DrawString("Discount ₹", boldFont, XBrushes.Black, colDiscount + 2, y + 15);
            y += rowHeight;

            decimal totalNet = 0;
            decimal totalDiscount = 0;

            // Product Rows
            foreach (var product in order.OrderProductViewModels)
            {
                decimal net = Math.Round(product.Price * product.Quantity, 2);
                decimal discountAmount = Math.Round((product.Price * (product.Discount / 100)) * product.Quantity, 2);

                totalNet += net;
                totalDiscount += discountAmount;

                gfx.DrawRectangle(XPens.Black, startX, y, tableWidth, rowHeight);

                gfx.DrawString(product.UniqueCode.ToString(), font, XBrushes.Black, colProductCode + 2, y + 15);
                gfx.DrawString(product.ProductName, font, XBrushes.Black, colDescription + 20, y + 15);
                gfx.DrawString(product.Quantity.ToString(), font, XBrushes.Black, colQty + 2, y + 15);
                gfx.DrawString($"{product.Price:F2}", font, XBrushes.Black, colUnit + 2, y + 15);
                gfx.DrawString($"{net:F2}", font, XBrushes.Black, colNet + 2, y + 15);
                gfx.DrawString($"{discountAmount:F2}", font, XBrushes.Black, colDiscount + 2, y + 15);
                y += rowHeight;

                if (y > page.Height - 60)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            decimal payableAmount = totalNet - totalDiscount;

            double summaryX = page.Width - 280;
            double summaryY = y + 40;
            double summaryLabelWidth = 120;
            double summaryValueWidth = 110;

            // Draw borders and text for 3 rows
            gfx.DrawRectangle(XPens.Black, summaryX, summaryY, summaryLabelWidth + summaryValueWidth, rowHeight * 3);

            // Row 1 - Total Net
            gfx.DrawRectangle(XPens.Black, summaryX, summaryY, summaryLabelWidth + summaryValueWidth, rowHeight);
            gfx.DrawRectangle(XPens.Black, summaryX, summaryY, summaryLabelWidth, rowHeight);
            gfx.DrawString("Total Net:", boldFont, XBrushes.Black, summaryX + 5, summaryY + 15);
            gfx.DrawString($"₹{totalNet:F2}", font, XBrushes.Black, summaryX + summaryLabelWidth + 5, summaryY + 15);

            // Total Discount
            gfx.DrawRectangle(XPens.Black, summaryX, summaryY + rowHeight, summaryLabelWidth + summaryValueWidth, rowHeight);
            gfx.DrawRectangle(XPens.Black, summaryX, summaryY + rowHeight, summaryLabelWidth, rowHeight);
            gfx.DrawString("Total Discount:", boldFont, XBrushes.Black, summaryX + 5, summaryY + rowHeight + 15);
            gfx.DrawString($"₹{totalDiscount:F2}", font, XBrushes.Black, summaryX + summaryLabelWidth + 5, summaryY + rowHeight + 15);

            // Total Payable
            gfx.DrawRectangle(XPens.Black, summaryX, summaryY + 2 * rowHeight, summaryLabelWidth + summaryValueWidth, rowHeight);
            gfx.DrawRectangle(XPens.Black, summaryX, summaryY + 2 * rowHeight, summaryLabelWidth, rowHeight);
            gfx.DrawString("Total Payable:", boldFont, XBrushes.Black, summaryX + 5, summaryY + 2 * rowHeight + 15);
            gfx.DrawString($"₹{payableAmount:F2}", new XFont("Verdana", 10, XFontStyle.Bold), XBrushes.Black,
                summaryX + summaryLabelWidth + 5, summaryY + 2 * rowHeight + 15);

            document.Save(stream, false);
            return File(stream.ToArray(), "application/pdf", $"Invoice_{order.OrderId}.pdf");
        }
    }

    private static string[] WrapText(string text, XFont font, XGraphics gfx, double maxWidth)
    {
        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            var size = gfx.MeasureString(testLine, font);
            if (size.Width > maxWidth)
            {
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
            lines.Add(currentLine);

        return lines.ToArray();
    }

}
