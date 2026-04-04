namespace BlogProjesi.Models;

// Uygulamada global bir hata çıktığında (Kullanıcıya gösterilen "Oops! Bir hata oluştu" sayfasında) kullanılan model.
public class ErrorViewModel
{
    // Hatanın sistemsel ID'sini (İzleme kimliğini) tutar.
    public string? RequestId { get; set; }

    // İzleme kimliği (RequestId) boş veya null değilse, özelliği ekranda gösterilsin (true) bilgisini döndürür.
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
