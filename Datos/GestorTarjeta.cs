
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Datos
{
    public class GestorTarjeta
    {

        public static IEnumerable<Tarjeta> Buscar(string Nombre, bool? Activo, int numeroPagina, out int RegistrosTotal)
        {

            //ref Entity Framework

            using (pav2Entities db = new pav2Entities())     //el using asegura el db.dispose() que libera la conexion de la base
            {
                IQueryable<Tarjeta> consulta = db.Tarjetas.Include(x => x.Tipo_Tarjeta);  // incluir obj hijos evitando lazy load (y tambien error de serializacion)

                // aplicar filtros
                //ref LinQ
                //Expresiones lambda, metodos de extension
                if (!string.IsNullOrEmpty(Nombre))
                    consulta = consulta.Where(x => x.Nombre.ToUpper().Contains(Nombre.ToUpper()));    // equivale al like '%TextoBuscar%'
                if (Activo != null)
                    consulta = consulta.Where(x => x.Activo == Activo);
                RegistrosTotal = consulta.Count();

                // ref EF; consultas paginadas
                int RegistroDesde = (numeroPagina - 1) * 10;
                var Lista = consulta.OrderBy(x => x.Nombre).Skip(RegistroDesde).Take(10).AsNoTracking().ToList(); // la instruccion sql recien se ejecuta cuando hacemos ToList()
                return Lista;
            }

        }




        public static Tarjeta BuscarPorId(int sId)
        {
            using (pav2Entities db = new pav2Entities())
            {
                return db.Tarjetas.Include(x => x.Tipo_Tarjeta).Where(x => x.IdTarjeta == sId).FirstOrDefault();
                //return db.Tarjeta.Find(sId);
            }
           
        }

        public static void Grabar(Tarjeta DtoSel)
        {
            // validar campos
            string erroresValidacion = "";
            if (string.IsNullOrEmpty(DtoSel.Nombre))
                erroresValidacion += "Nombre es un dato requerido; ";
            if (DtoSel.Limite == null || DtoSel.Limite == 0)
                erroresValidacion += "Limite es un dato requerido; ";
            if (!string.IsNullOrEmpty(erroresValidacion))
                throw new Exception(erroresValidacion);

            // grabar registro
            using (pav2Entities db = new pav2Entities())
            {
                try
                {
                    if (DtoSel.IdTarjeta != 0)
                    {
                        db.Entry(DtoSel).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        IQueryable<Tarjeta> consulta = db.Tarjetas.Include(x => x.Tipo_Tarjeta);
                        int total = consulta.Count();
                        DtoSel.IdTarjeta = total + 1;
                        db.Tarjetas.Add(DtoSel);
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    if (ex.ToString().Contains("UK_Tarjeta_Nombre"))
                        throw new ApplicationException("Ya existe otra Tarjeta con ese Nombre");
                    else
                        throw;
                }
            }
        }


        public static void ActivarDesactivar(int IdTarjeta)
        {
            using (pav2Entities db = new pav2Entities())
            { 
                //ref Entity Framework; ejecutar codigo sql directo
                db.Database.ExecuteSqlCommand("Update Tarjetas set Activo = case when ISNULL(activo,1)=1 then 0 else 1 end  where IdTarjeta = @IdTarjeta",
                    new SqlParameter("@IdTarjeta", IdTarjeta)
                    );
            }
        }



        public static Tarjeta ADOBuscarPorId(int IdTarjeta)
        {   
            //ref ADO; Recuperar cadena de conexión de web.config
            string CadenaConexion = System.Configuration.ConfigurationManager.ConnectionStrings["PymesAdo"].ConnectionString;
            //ref ADO; objetos conexion, comando, parameters y datareader
            SqlConnection cn = new SqlConnection();
            cn.ConnectionString = CadenaConexion;
            Tarjeta art = null;
            try
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = cn;
                cmd.CommandText = "select * from Tarjeta c where c.IdTarjeta = @IdTarjeta";
                cmd.Parameters.Add(new SqlParameter("@IdTarjeta", IdTarjeta));
                SqlDataReader dr = cmd.ExecuteReader();
                // con el resultado cargar una entidad
                //ref ADO; generar un objeto entidad
                if (dr.Read())
                {
                    art = new Tarjeta();
                    art.IdTarjeta = (int)dr["IdTarjeta"];
                    art.Nombre = dr["Nombre"].ToString();
                    if (dr["Limite"] != DBNull.Value)
                        art.Limite = (decimal)dr["Limite"];
                    if (dr["Numero"] != DBNull.Value)
                      art.Numero = dr["Numero"].ToString();
                    if (dr["IdTipoTarjeta"] != DBNull.Value)
                        art.IdTipoTarjeta = (int)dr["IdTipoTarjeta"];
                    if (dr["Activo"] != DBNull.Value)
                        art.Activo = (bool)dr["Activo"];
                    if (dr["FechaAlta"] != DBNull.Value)
                        art.FechaAlta = (DateTime)dr["FechaAlta"];
                }
                dr.Close();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();
            }
            return art;
        }
    }
}

