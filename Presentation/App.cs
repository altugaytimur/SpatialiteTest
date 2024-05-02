using Business.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation
{
    public class App
    {
        private readonly IBuiildingService _buildingService;
        private readonly IDoorService _doorService;

        public App(IBuiildingService buildingService, IDoorService doorService)
        {
            _buildingService = buildingService;
            _doorService = doorService;
        }

        public void Run()
        {
            while (true)
            {
                Console.WriteLine("1. Binaları Oluştur\n2. Kapı Kontrolü\n3. Kapı Sonuçlarını Yazdır\n4. Çıkış Yap\nLütfen bir seçenek giriniz:");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        _buildingService.CreateBuildingsFromNodes();
                        break;
                    case "2":
                        _doorService.CheckDoorsInsideBuildings();
                        break;
                    case "3":
                        _doorService.PrintDoorResults();
                        break;
                    case "4":
                        Console.WriteLine("Programdan çıkılıyor...");
                        return;
                    default:
                        Console.WriteLine("Geçersiz seçenek. Lütfen tekrar deneyin.");
                        break;
                }
            }
        }
    }
}
