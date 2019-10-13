using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Datos;

namespace WebTarjetas.Controllers
{
    public class TarjetasController : ApiController
    {
        //ref webapi; por defecto se busca el metodo de request (get, post, etc) segun comienze el nombre de la accion y sus parametros 
        //GET: api/Tarjetas
        public IHttpActionResult GetTarjetas(string Nombre = "", bool? Activo = null, int numeroPagina = 1)
        {
            //ref webapi parametros;
            //ref webapi tipos de retorno de los metodos; cambiamos la devolucion generica del metodo: IQueryable<Tarjetas> por IHttpActionResult para poder devolver tambien RegistrosTotal
            int RegistrosTotal;
            //ref c#  var
            var Lista = Datos.GestorTarjeta.Buscar(Nombre, Activo, numeroPagina, out RegistrosTotal);
            return Ok(new { Lista = Lista, RegistrosTotal = RegistrosTotal });
        }


        // GET: api/Tarjetas/5
        [ResponseType(typeof(Tarjeta))]
        public IHttpActionResult GetTarjetas(int id)
        {
            Tarjeta tarjetas = Datos.GestorTarjeta.BuscarPorId(id);
            //ref Ado generar objeto generico
            //Tarjetas tarjetas = Datos.GestorTarjetas.ADOBuscarPorId(id);
            if (tarjetas == null)
            {
                return NotFound();  // status 404
            }
            return Ok(tarjetas);
        }

        // PUT: api/Tarjetas/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutTarjetas(int id, Tarjeta tarjetas)
        {
            if (!ModelState.IsValid)  //ref DataAnnotations; validar en el servidor ??
            {
                return BadRequest(ModelState);
            }

            if (id != tarjetas.IdTarjeta)
            {
                return BadRequest();
            }

            Datos.GestorTarjeta.Grabar(tarjetas);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Tarjetas
        [ResponseType(typeof(Tarjeta))]
        public IHttpActionResult PostTarjetas(Tarjeta tarjetas)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);

            }

            Datos.GestorTarjeta.Grabar(tarjetas);

            return CreatedAtRoute("DefaultApi", new { id = tarjetas.IdTarjeta }, tarjetas);
        }

        //DELETE: api/Tarjetas/1 
        [ResponseType(typeof(Tarjeta))]
        public IHttpActionResult DeleteTarjetas(int id)
        {
            //new ApplicationException("error en base");   //ref??? throw no genera error dentro de webapi, continua normalmente?

            //ref EntityFramework baja logica
            Datos.GestorTarjeta.ActivarDesactivar(id);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}