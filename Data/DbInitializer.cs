using ACCOB.Models;
using Microsoft.EntityFrameworkCore;

namespace ACCOB.Data
{
    public static class DbInitializer
    {
        public static async Task SeedWinData(ApplicationDbContext context)
        {
            // Solo insertamos si la tabla de zonas está vacía
            if (await context.Zonas.AnyAsync()) return;

            var zonas = new List<Zona>
            {
                new Zona { Nombre = "Plan Lima / Callao" },
                new Zona { Nombre = "Plan Provincias" },
                new Zona { Nombre = "Plan GAMER (Nacional)" },
                new Zona { Nombre = "Plan XGSPON" },
                new Zona { Nombre = "Plan Destacadas Lima" },
                new Zona { Nombre = "Plan Destacadas Hogar" }
            };
            context.Zonas.AddRange(zonas);
            await context.SaveChangesAsync();

            var zona1 = zonas[0]; //Lima y Callao
            var zona2 = zonas[1]; //Provincias
            var zona3 = zonas[2]; //Gamer
            var zona4 = zonas[3]; //XGSPON
            var zona5 = zonas[4]; //Destacadas Lima
            var zona6 = zonas[5]; //Condominios y edificios

            var planes = new List<PlanWin>
            {
                //Planes Lima / Callao
                new PlanWin { Nombre = "WIN Foco (Internet + Wintv + L1MAX)", ZonaId = zona1.Id },
                new PlanWin { Nombre = "100% FIBRA (Solo Internet)", ZonaId = zona1.Id },
                new PlanWin { Nombre = "100% FIBRA Plus (Internet + Wintv)", ZonaId = zona1.Id },
                new PlanWin { Nombre = "100% FIBRA Premium (Internet + Wintv)", ZonaId = zona1.Id },
                new PlanWin { Nombre = "100% FIBRA MAX (Internet + Wintv + L1MAX)", ZonaId = zona1.Id },
                new PlanWin { Nombre = "100% FIBRA MAX Premium (Internet + Wintv + L1MAX)", ZonaId = zona1.Id },
                new PlanWin { Nombre = "100% FIBRA DGO (Internet + DGO Basico)", ZonaId = zona1.Id },
                new PlanWin { Nombre = "100% FIBRA DGO Full (Internet + DGO Full)", ZonaId = zona1.Id },
                new PlanWin { Nombre = "100% FIBRA RUC20 (Solo Internet)", ZonaId = zona1.Id },

                //Planes Provincias
                new PlanWin { Nombre = "WIN Foco (Internet + Wintv + L1MAX)", ZonaId = zona2.Id },
                new PlanWin { Nombre = "100% FIBRA (Solo Internet)", ZonaId = zona2.Id },
                new PlanWin { Nombre = "100% FIBRA RUC20 (Solo Internet)", ZonaId = zona2.Id },

                //Planes Gamer (Nacional)
                new PlanWin { Nombre = "WIN Gamer (Solo Internet)", ZonaId = zona3.Id },

                //Planes XGSPON (Nacional)
                new PlanWin { Nombre = "WIN 100% FIBRA (Solo Internet)", ZonaId = zona4.Id },

                //Planes Destacadas Lima
                new PlanWin { Nombre = "100% FIBRA Premium (Internet + Wintv)", ZonaId = zona5.Id },
                new PlanWin { Nombre = "100% FIBRA MAX (Internet + Wintv + L1MAX)", ZonaId = zona5.Id },
                new PlanWin { Nombre = "100% FIBRA MAX Premium (Internet + Wintv + L1MAX)", ZonaId = zona5.Id },

                //Planes Destacadas Hogar
                new PlanWin { Nombre = "100% FIBRA (Solo Internet)", ZonaId = zona6.Id },
                new PlanWin { Nombre = "100% FIBRA Plus (Internet + Wintv)", ZonaId = zona6.Id },
                new PlanWin { Nombre = "100% FIBRA Premium (Internet + Wintv)", ZonaId = zona6.Id },
                new PlanWin { Nombre = "100% FIBRA MAX (Internet + Wintv + L1MAX)", ZonaId = zona6.Id },
                new PlanWin { Nombre = "100% FIBRA MAX Premium (Internet + Wintv + L1MAX)", ZonaId = zona6.Id },
            };
            context.PlanesWin.AddRange(planes);
            await context.SaveChangesAsync();

            //Plan Lima / Callao
            var planFoco = planes[0];
            var planFibra = planes[1];
            var planFibraPlus = planes[2];
            var planFibraPremium = planes[3];
            var planFibraMax = planes[4];
            var planFibraMaxPremium = planes[5];
            var planFibraDGO = planes[6];
            var planFibraDGOFull = planes[7];
            var planFibraRUC20 = planes[8];

            //Plan Provincias
            var planFocoPro = planes[9];
            var planFibraPro = planes[10];
            var planFibraRUC20Pro = planes[11];

            //Plan Gamer (Nacional)
            var planGamer = planes[12];

            //Plan XGSPON
            var planXGSPON = planes[13];

            //Plan Destacadas Lima
            var planFibraDes = planes[14];
            var planFibraMaxDes = planes[15];
            var planFibraMaxPremiumDes = planes[16];

            //Planes Destacadas Hogar
            var planFibraHog = planes[17];
            var planFibraPlusHog = planes[18];
            var planFibraPremiumHog = planes[19];
            var planFibraMaxHog = planes[20];
            var planFibraMaxPremiumHog = planes[21];

            context.TarifasPlan.AddRange(new List<TarifaPlan>
            {
                //Planes Lima / Callao - Plan Foco
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 159.90m, PrecioPromocional = 79.95m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFoco.Id },

                //Planes Lima / Callao - 100% Fibra
                new TarifaPlan { Velocidad = "750 Mbps", PrecioRegular = 109m, PrecioPromocional = 109m, DescripcionDescuento = "Precio Fijo", PlanWinId = planFibra.Id },
                new TarifaPlan { Velocidad = "850 Mbps", PrecioRegular = 119m, PrecioPromocional = 64.95m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFibra.Id },
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 139m, PrecioPromocional = 69.95m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFibra.Id },

                //Planes Lima / Callao - 100% Fibra Plus
                new TarifaPlan { Velocidad = "850 Mbps", PrecioRegular = 129.90m, PrecioPromocional = 64.95m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFibraPlus.Id },
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 139.90m, PrecioPromocional = 69.95m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFibraPlus.Id },

                //Planes Lima / Callao - 100% Fibra Premium
                new TarifaPlan { Velocidad = "850 Mbps", PrecioRegular = 129.90m, PrecioPromocional = 64.95m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFibraPremium.Id },

                //Planes Lima / Callao - 100% Fibra Max
                new TarifaPlan { Velocidad = "850 Mbps", PrecioRegular = 139.90m, PrecioPromocional = 69.95m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFibraMax.Id },
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 149.90m, PrecioPromocional = 74.95m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFibraMax.Id },
                
                //Planes Lima / Callao - 100% Fibra Max Premium
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 159.90m, PrecioPromocional = 79.95m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFibraMaxPremium.Id },

                //Planes Lima / Callao - 100% Fibra DGO
                new TarifaPlan { Velocidad = "850 Mbps", PrecioRegular = 169.90m, PrecioPromocional = 84.95m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFibraDGO.Id },

                //Planes Lima / Callao - 100% Fibra DGO Full
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 189.90m, PrecioPromocional = 94.95m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFibraDGOFull.Id },

                //Planes Lima / Callao - 100% Fibra RUC20
                new TarifaPlan { Velocidad = "750 Mbps", PrecioRegular = 109m, PrecioPromocional = 109m, DescripcionDescuento = "Precio Fijo", PlanWinId = planFibraRUC20.Id },
                new TarifaPlan { Velocidad = "850 Mbps", PrecioRegular = 119m, PrecioPromocional = 119m, DescripcionDescuento = "Precio Fijo", PlanWinId = planFibraRUC20.Id },
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 139m, PrecioPromocional = 139m, DescripcionDescuento = "Precio Fijo", PlanWinId = planFibraRUC20.Id },

                //Planes Provincias - Plan Foco
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 129.90m, PrecioPromocional = 64.95m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFocoPro.Id },

                //Planes Provincias - 100% Fibra
                new TarifaPlan { Velocidad = "550 Mbps", PrecioRegular = 89m, PrecioPromocional = 44.50m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFibraPro.Id },
                new TarifaPlan { Velocidad = "750 Mbps", PrecioRegular = 99m, PrecioPromocional = 49.50m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFibraPro.Id },
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 129m, PrecioPromocional = 64.50m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planFibraPro.Id },

                //Planes Provincias - 100% Fibra RUC20
                new TarifaPlan { Velocidad = "550 Mbps", PrecioRegular = 89m, PrecioPromocional = 89m, DescripcionDescuento = "Precio Fijo", PlanWinId = planFibraRUC20Pro.Id },
                new TarifaPlan { Velocidad = "750 Mbps", PrecioRegular = 99m, PrecioPromocional = 99m, DescripcionDescuento = "Precio Fijo", PlanWinId = planFibraRUC20Pro.Id },
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 129m, PrecioPromocional = 129m, DescripcionDescuento = "Precio Fijo", PlanWinId = planFibraRUC20Pro.Id },

                //Planes Gamer (Nacional)
                new TarifaPlan { Velocidad = "600 Mbps", PrecioRegular = 129m, PrecioPromocional = 64.50m, DescripcionDescuento = "50% desc. x 2 mes", PlanWinId = planGamer.Id },
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 159m, PrecioPromocional = 79.50m, DescripcionDescuento = "50% desc. x 2 mes", PlanWinId = planGamer.Id },

                //Planes XGSPON
                new TarifaPlan { Velocidad = "1500 Mbps", PrecioRegular = 189m, PrecioPromocional = 94.50m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planXGSPON.Id },
                new TarifaPlan { Velocidad = "2000 Mbps", PrecioRegular = 219m, PrecioPromocional = 109.50m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planXGSPON.Id },
                new TarifaPlan { Velocidad = "2500 Mbps", PrecioRegular = 239m, PrecioPromocional = 119.50m, DescripcionDescuento = "50% desc. x 1 mes", PlanWinId = planXGSPON.Id },

                //Planes Destacadas Lima - 100% Fibra Premium
                new TarifaPlan { Velocidad = "850 Mbps", PrecioRegular = 129.90m, PrecioPromocional = 65.95m, DescripcionDescuento = "50% desc. x 2 meses", PlanWinId = planFibraDes.Id },

                //Planes Destacadas Lima - 100% Fibra Max
                new TarifaPlan { Velocidad = "850 Mbps", PrecioRegular = 139.90m, PrecioPromocional = 69.95m, DescripcionDescuento = "50% desc. x 2 meses", PlanWinId = planFibraMaxDes.Id },
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 149.90m, PrecioPromocional = 74.95m, DescripcionDescuento = "50% desc. x 2 meses", PlanWinId = planFibraMaxDes.Id },

                //Planes Destacadas Lima - 100% Fibra Max Premium
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 159.90m, PrecioPromocional = 79.95m, DescripcionDescuento = "50% desc. x 2 meses", PlanWinId = planFibraMaxPremiumDes.Id },
                
                //Planes Destacadas Hogar - 100% Fibra
                new TarifaPlan { Velocidad = "750 Mbps", PrecioRegular = 109m, PrecioPromocional = 109m, DescripcionDescuento = "Precio Fijos", PlanWinId = planFibraHog.Id },
                new TarifaPlan { Velocidad = "850 Mbps", PrecioRegular = 119m, PrecioPromocional = 1m, DescripcionDescuento = "A un S/.1.00 x 2 meses", PlanWinId = planFibraHog.Id },
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 139m, PrecioPromocional = 1m, DescripcionDescuento = "A un S/.1.00 x 2 meses", PlanWinId = planFibraHog.Id },

                //Planes Destacadas Lima - 100% Fibra Plus
                new TarifaPlan { Velocidad = "850 Mbps", PrecioRegular = 119.90m, PrecioPromocional = 1m, DescripcionDescuento = "A un S/.1.00 x 2 meses", PlanWinId = planFibraPlusHog.Id },
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 139.90m, PrecioPromocional = 1m, DescripcionDescuento = "A un S/.1.00 x 2 meses", PlanWinId = planFibraPlusHog.Id },
                //Planes Destacadas Lima - 100% Fibra Premium
                new TarifaPlan { Velocidad = "850 Mbps", PrecioRegular = 129.90m, PrecioPromocional = 1m, DescripcionDescuento = "A un S/.1.00 x 2 meses", PlanWinId = planFibraPremiumHog.Id },
                //Planes Destacadas Lima - 100% Fibra Max
                new TarifaPlan { Velocidad = "850 Mbps", PrecioRegular = 139.90m, PrecioPromocional = 1m, DescripcionDescuento = "A un S/.1.00 x 2 meses", PlanWinId = planFibraMaxHog.Id },
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 149.90m, PrecioPromocional = 1m, DescripcionDescuento = "A un S/.1.00 x 2 meses", PlanWinId = planFibraMaxHog.Id },

                //Planes Destacadas Lima - 100% Fibra Max Premium
                new TarifaPlan { Velocidad = "1000 Mbps", PrecioRegular = 159.90m, PrecioPromocional = 1m, DescripcionDescuento = "A un S/.1.00 x 2 meses", PlanWinId = planFibraMaxPremiumHog.Id },
            });

            await context.SaveChangesAsync();
        }
    }
}