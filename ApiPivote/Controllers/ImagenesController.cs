using Microsoft.AspNetCore.Mvc;
using RestSharp;
using ApiPivote.Models;

namespace ApiPivote.Controllers
{
    //Autor: Alejandro Hurtado Mijango
    [ApiController]
    [Route("[controller]")]
    public class ImagenesController : ControllerBase
    {
        private readonly ILogger<ImagenesController> _logger;
        string urlBase = "https://apitest-bt.herokuapp.com/api/v1/imagenes";

        public ImagenesController(ILogger<ImagenesController> logger)
        {
            _logger = logger;
        }

        //Subir Imagen
        [HttpPost(Name = "SubirImagen")]
        public ActionResult Post(EntradaImagenPost entrada)
        {
            if (entrada == null)
                return BadRequest("Informacion de entrada basica no recibida!");

            if (string.IsNullOrEmpty(entrada.nombre))
                return BadRequest("Ingrese el nombre de la imagen!");

            if (string.IsNullOrEmpty(entrada.base64))
                return BadRequest("Ingrese la cadena de la imagen en formato base64!");


            using (var clienteRest = this.ObtenerRestCliente())
            {
                var solicitudPost = this.ObtenerRestRequest();
                solicitudPost.Method = Method.Post;
                solicitudPost.AddJsonBody(entrada);

                try
                {
                    var respuestaBanco = clienteRest.Post<InfoImagen>(solicitudPost);
                    if (respuestaBanco == null || string.IsNullOrEmpty(respuestaBanco.created_at))
                        return BadRequest("No se pudo subir la imagen!");

                    return Ok("Imagen subida correctamente!");
                }
                catch (Exception error)
                {
                    _logger.LogError(error.Message);

                    return BadRequest(error.Message);
                }
            }
        }

        //Listar todas las Imagenes
        [HttpGet(Name = "GetImagenes")]
        public ActionResult<List<InfoImagen>> Get()
        {
            var resultado = new List<InfoImagen>();

            using (var clienteRest = this.ObtenerRestCliente())
            {
                var solicitudGet = this.ObtenerRestRequest();
                solicitudGet.Method = Method.Get;

                try
                {
                    var respuestaBanco = clienteRest.Get<List<InfoImagen>>(solicitudGet);
                    if (respuestaBanco == null)
                        return new StatusCodeResult(503);

                    foreach (var itemBanco in respuestaBanco)
                    {
                        try
                        {

                            if (itemBanco.id != default(int)
                                && (itemBanco.nombre.Contains(".png") || itemBanco.nombre.Contains(".jpg") || itemBanco.nombre.Contains(".jpeg") || itemBanco.nombre.Contains(".gif") || itemBanco.nombre.Contains(".svg"))
                                && !string.IsNullOrEmpty(itemBanco.base64))
                            {
                                resultado.Add(itemBanco);
                            }
                        }
                        catch (Exception errorItem)
                        {
                            _logger.LogError(errorItem, "Error en Item de Imagen!");
                        }
                    }

                    if (resultado.Count == default(int))
                        return NoContent();
                }
                catch (Exception errorGeneral)
                {
                    _logger.LogError(errorGeneral, "Error General!");
                }
            }

            return resultado;
        }

        //Obtiene imagen por Id
        [HttpGet("{id:int}")]
        public ActionResult<InfoImagen> GetById(int? id)
        {
            if (id == null ||  id == default(int))
                return BadRequest("Debe especificar el id de la imagen!");

            using (var clienteRest = this.ObtenerRestCliente())
            {
                clienteRest.Options.BaseUrl = new Uri(string.Format("{0}/{1}", this.urlBase, id));
                var solicitudGet = this.ObtenerRestRequest();
                solicitudGet.Method = Method.Get;

                try
                {
                    var infoImagenBanco = clienteRest.Get<InfoImagen>(solicitudGet);
                    if (infoImagenBanco == null)
                        return new StatusCodeResult(500);

                    if (!string.IsNullOrEmpty(infoImagenBanco.base64) && !string.IsNullOrEmpty(infoImagenBanco.nombre))
                    {
                        var bytesImagen = Convert.FromBase64String(infoImagenBanco.base64);
                        var nombreImagen = infoImagenBanco.nombre;
                        var extensionImagen = infoImagenBanco.nombre.Split(".")[1].Replace(".","");

                        return File(bytesImagen, string.Format("image/{0}", extensionImagen), nombreImagen);
                    }

                    return infoImagenBanco;
                }
                catch (Exception error)
                {
                    _logger.LogError(error, "GetById");
                }
            }

            return NotFound();
        }

        //Modificar imagen
        [HttpPut("{id:int}")]
        public ActionResult Put(int? id, EntradaImagenPost entrada)
        {
            if (id == null || id == default(int))
                return BadRequest("Debe especificar el id de la imagen!");

            if (entrada == null)
                return BadRequest("Informacion de entrada basica no recibida!");

            using (var clienteRest = this.ObtenerRestCliente())
            {
                clienteRest.Options.BaseUrl = new Uri(string.Format("{0}/{1}", this.urlBase, id));
                var solicitudPut = this.ObtenerRestRequest();
                solicitudPut.Method = Method.Put;
                solicitudPut.AddJsonBody(entrada);

                try
                {
                    var respuestaBanco = clienteRest.Put<InfoImagen>(solicitudPut);
                    if (respuestaBanco == null || string.IsNullOrEmpty(respuestaBanco.created_at))
                        return BadRequest("No se pudo subir la imagen!");

                    return Ok("Imagen modificada correctamente!");
                }
                catch (Exception error)
                {
                    _logger.LogError(error.Message);

                    return BadRequest(error.Message);
                }
            }
        }

        private RestClient ObtenerRestCliente()
        {
            return new RestClient(this.urlBase);
        }

        private RestRequest ObtenerRestRequest()
        {
            var resultado = new RestRequest();
            resultado.AddHeader("user", "User123");
            resultado.AddHeader("password", "Password123");

            return resultado;
        }
    }
}
