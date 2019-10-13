
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
    public class GestorArticulos
    {

        public static IEnumerable<Articulos> Buscar(string Nombre, bool? Activo, int numeroPagina, out int RegistrosTotal)
        {

            //ref Entity Framework

            using (PymesEntities db = new PymesEntities())     //el using asegura el db.dispose() que libera la conexion de la base
            {
                IQueryable<Articulos> consulta = db.Articulos.Include(x => x.ArticulosFamilias);  // incluir obj hijos evitando lazy load (y tambien error de serializacion)

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



        public static Articulos BuscarPorId(int sId)
        {
            using (PymesEntities db = new PymesEntities())
            {
                return db.Articulos.Include(x => x.ArticulosFamilias).Where(x => x.IdArticulo == sId).FirstOrDefault();
                //return db.Articulos.Find(sId);
            }

        }

        public static void Grabar(Articulos DtoSel)
        {
            // validar campos
            string erroresValidacion = "";
            if (string.IsNullOrEmpty(DtoSel.Nombre))
                erroresValidacion += "Nombre es un dato requerido; ";
            if (DtoSel.Precio == null || DtoSel.Precio == 0)
                erroresValidacion += "Precio es un dato requerido; ";
            if (!string.IsNullOrEmpty(erroresValidacion))
                throw new Exception(erroresValidacion);

            // grabar registro
            using (PymesEntities db = new PymesEntities())
            {
                try
                {
                    if (DtoSel.IdArticulo != 0)
                    {
                        db.Entry(DtoSel).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        db.Articulos.Add(DtoSel);
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    if (ex.ToString().Contains("UK_Articulos_Nombre"))
                        throw new ApplicationException("Ya existe otro Artículo con ese Nombre");
                    else
                        throw;
                }
            }
        }


        public static void ActivarDesactivar(int IdArticulo)
        {
            using (PymesEntities db = new PymesEntities())
            {
                //ref Entity Framework; ejecutar codigo sql directo
                db.Database.ExecuteSqlCommand("Update Articulos set Activo = case when ISNULL(activo,1)=1 then 0 else 1 end  where IdArticulo = @IdArticulo",
                    new SqlParameter("@IdArticulo", IdArticulo)
                    );
            }
        }



        public static Articulos ADOBuscarPorId(int IdArticulo)
        {
            //ref ADO; Recuperar cadena de conexión de web.config
            string CadenaConexion = System.Configuration.ConfigurationManager.ConnectionStrings["PymesAdo"].ConnectionString;
            //ref ADO; objetos conexion, comando, parameters y datareader
            SqlConnection cn = new SqlConnection();
            cn.ConnectionString = CadenaConexion;
            Articulos art = null;
            try
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = cn;
                cmd.CommandText = "select * from Articulos c where c.idArticulo = @IdArticulo";
                cmd.Parameters.Add(new SqlParameter("@IdArticulo", IdArticulo));
                SqlDataReader dr = cmd.ExecuteReader();
                // con el resultado cargar una entidad
                //ref ADO; generar un objeto entidad
                if (dr.Read())
                {
                    art = new Articulos();
                    art.IdArticulo = (int)dr["IdArticulo"];
                    art.Nombre = dr["nombre"].ToString();
                    if (dr["Precio"] != DBNull.Value)
                        art.Precio = (decimal)dr["Precio"];
                    if (dr["CodigoDeBarra"] != DBNull.Value)
                        art.CodigoDeBarra = dr["CodigoDeBarra"].ToString();
                    if (dr["IdArticuloFamilia"] != DBNull.Value)
                        art.IdArticuloFamilia = (int)dr["IdArticuloFamilia"];
                    if (dr["Stock"] != DBNull.Value)
                        art.Stock = (int)dr["Stock"];
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

