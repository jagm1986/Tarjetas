//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Datos
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tarjeta
    {
        public int IdTarjeta { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Numero { get; set; }
        public decimal Limite { get; set; }
        public int IdTipoTarjeta { get; set; }
        public System.DateTime FechaAlta { get; set; }
        public bool Activo { get; set; }
    
        public virtual Tipo_Tarjeta Tipo_Tarjeta { get; set; }
    }
}
