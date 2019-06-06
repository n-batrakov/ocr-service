using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OcrService.Core;

namespace OcrService.Implementations
{
    public class FakeOcr : IOcrClient
    {
        private const string Text = "Для администрирования и настройки ПС КО УОЭ ИТС Банка России в подразделениях информатизации требуется наличие минимальной конфигурации аппаратных и программных средств: Требования к серверным техническим и программным средствам: серверная операционная система и программное обеспечение: операционная система MS Windows Server 2012 R2 и выше; Microsoft SQL Server 2016 и выше; Microsoft Reporting Services 2016; Microsoft.Net 4.6.2 Runtime; Internet Information Services 8.5 и выше. серверное аппаратное обеспечение (трeбования): совместимый с MS Windows Server 2012 R2 компьютер на базе процессора не ниже Pentium 4 с тактовой частотой 2х2 ГГц, 4 ГБ ОЗУ, 100 ГБ свободного дискового пространства; монитор, обеспечивающий разрешение изображения не менее 1280*1024 точек (1440*900 точек для широкоэкранных моделей), с видимой диагональю не менее 19”. Требования к техническим средствам эксплуатирующего персонала: клиентское аппаратное обеспечение: процессор Intel Pentium III 866 МГц и выше; оперативная память 512 Мбайт и выше (рекомендуется 1024 Мбайт); жесткий диск (свободное место на диске 5 Гб); SVGA дисплей; Скорость подключения к серверной части – 2 Мбит/сек. клиентская операционная система: MS Windows 7/8/10. клиентское программное обеспечение: Microsoft Internet Explorer 10 и выше. Microsoft Office 2010 Excel; Microsoft Office 2010 Word. Для обеспечения серверной платформы возможно применение технологии виртуальных машин в рамках регионального СКСД.";

        private FakeOcr()
        {
        }
        
        public static readonly IOcrClient Instance = new FakeOcr();
        
        public Task<OcrResult> RecognizeAsync(Stream image, CancellationToken token)
        {
            return Task.FromResult(OcrResult.Success(Text));
        }
    }
}