using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantAPI.Models
{
    public enum MerchType
    {
        [Display(Name = "Склад")]
        WareHouse = 0,
        [Display(Name = "ПВЗ")]
        ПВЗ = 1,
        [Display(Name = "Постамат")]
        Постамат = 2
    }

    [Table("TMP_MERCHANTS")]
    public class MerchantModel
    {
        [Key] public int Id { get; set; }

        [Required]
        public int Client_ID { get; set; }

        [MaxLength(41)]
        [Required]
        public string Merch_Code { get; set; }
        [MaxLength(100)]
        [Required]
        public string Merch_Name { get; set; }
        [MaxLength(50)]
        public string Last_Name { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Patr_Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }


        public int Is_Actual { get; set; }

        public MerchType M_Type { get; set; }

        [MaxLength(30)]
        public string Country { get; set; }

        [MaxLength(200)]
        public string Region { get; set; }

        [MaxLength(300)]
        [Required]
        public string City { get; set; }

        [MaxLength(200)]
        [Required]
        public string Street { get; set; }

        [MaxLength(20)]
        public string Home { get; set; }

        [MaxLength(30)]
        public string Corp { get; set; }

        [MaxLength(254)]
        public string Office { get; set; }

        [MaxLength(60)]
        [Required]
        public string Phone_1 { get; set; }

        [MaxLength(60)]
        public string Phone_2 { get; set; }

        public DateTime? Date_Create { get; set; }

        public MerchantModel()
        {
            Client_ID = 206758;
            Date_Create = DateTime.Now;
            Is_Actual = 1;
        }
    }
}
