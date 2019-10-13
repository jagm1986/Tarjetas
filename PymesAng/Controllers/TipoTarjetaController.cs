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

namespace WebTarjetas
{
    public class TipoTarjetaController : ApiController
    {
        private pav2Entities db = new pav2Entities();

        // GET: api/TipoTarjeta
        public IQueryable<Tipo_Tarjeta> GetTipoTarjeta()
        {
            return db.Tipo_Tarjeta;
        }

        // GET: api/ArticulosFamilias/5
        [ResponseType(typeof(Tipo_Tarjeta))]
        public IHttpActionResult GetTipoTarjeta(int id)
        {
            Tipo_Tarjeta tipoTarjeta = db.Tipo_Tarjeta.Find(id);
            if (tipoTarjeta == null)
            {
                return NotFound();
            }

            return Ok(tipoTarjeta);
        }

        // PUT: api/TipoTarjeta/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutArticulosFamilias(int id, Tipo_Tarjeta tipoTarjeta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tipoTarjeta.IdTipoTarjeta)
            {
                return BadRequest();
            }

            db.Entry(tipoTarjeta).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoTarjetaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/ArticulosFamilias
        [ResponseType(typeof(Tipo_Tarjeta))]
        public IHttpActionResult PostTipoTarjeta(Tipo_Tarjeta tipoTarjeta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Tipo_Tarjeta.Add(tipoTarjeta);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = tipoTarjeta.IdTipoTarjeta }, tipoTarjeta);
        }

        // DELETE: api/TipoTarjeta/5
        [ResponseType(typeof(Tipo_Tarjeta))]
        public IHttpActionResult DeleteTipoTarjeta(int id)
        {
            Tipo_Tarjeta tipoTarjeta = db.Tipo_Tarjeta.Find(id);
            if (tipoTarjeta == null)
            {
                return NotFound();
            }

            db.Tipo_Tarjeta.Remove(tipoTarjeta);
            db.SaveChanges();

            return Ok(tipoTarjeta);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TipoTarjetaExists(int id)
        {
            return db.Tipo_Tarjeta.Count(e => e.IdTipoTarjeta == id) > 0;
        }
    }
}