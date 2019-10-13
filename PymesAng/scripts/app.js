//var myApp = angular.module('myApp', []);
var myApp = angular.module('myApp', ['ui.bootstrap']);

myApp.service('myService', function ($timeout) {
    this.Alert = function (dialogText, dialogTitle) {
        var alertModal = $('<div class="modal fade" id="myModal"> <div class="modal-dialog"> <div class="modal-content"> <div class="modal-header"> <h4>' + (dialogTitle || 'Atención') + '</h4> <button type="button" class="close" data-dismiss="modal">&times;</button> </div> <div class="modal-body"><p>' + dialogText + '</p></div><div class="modal-footer"><button type="button" class="btn" data-dismiss="modal">Cerrar</button></div></div></div></div>');
        $timeout(function () { alertModal.modal(); });
    };

    this.Confirm = function (dialogText, okFunc, cancelFunc, dialogTitle, but1, but2) {
        var confirmModal = $('<div class="modal fade" id="myModal"> <div class="modal-dialog"> <div class="modal-content"> <div class="modal-header"> <h4>' + dialogTitle + '</h4> <button type="button" class="close" data-dismiss="modal">&times;</button> </div> <div class="modal-body"><p>' + dialogText + '</p></div><div class="modal-footer"><button ID="SiBtn" class="btn" data-dismiss="modal">' + (but1 == undefined ? 'Si' : but1) + '</button><button type="button" ID="NoBtn" class="btn" data-dismiss="modal">' + (but2 == undefined ? 'No' : but2) + '</button></div></div></div></div >');
        confirmModal.find('#SiBtn').click(function (event) {
            if (okFunc)
                okFunc();
            confirmModal.modal('hide');
        });
        confirmModal.find('#NoBtn').click(function (event) {
            if (cancelFunc)
                cancelFunc();
            confirmModal.modal('hide');
        });
        $timeout(function () { confirmModal.modal(); });
    };
    // bloqueo / desbloqueo de pantalla
    // https://www.w3schools.com/bootstrap4/bootstrap_modal.asp
    // https://www.w3schools.com/bootstrap4/bootstrap_progressbars.asp
    var contadorBloqueo = 0;
    var $dialog = $(`
   <div class="modal" id="myModal">
    <div class="modal-dialog">
      <div class="modal-content">
        <!-- Modal Header -->
        <div class="modal-header">
          <h5 class="modal-title">Espere por favor..</h5>
        </div>
        <!-- Modal body -->
        <div class="modal-body">
	        <div class="progress">
        		  <div class="progress-bar progress-bar-striped progress-bar-animated" style="width:100%">
    	     </div>	
        </div>
      </div>
    </div>
  </div>
`);
    this.BloquearPantalla = function () {
        contadorBloqueo++;
        if (contadorBloqueo == 1)
            $dialog.modal({
                backdrop: 'static',
                keyboard: false
            });
    };
    this.DesbloquearPantalla = function () {
        contadorBloqueo--;
        if (contadorBloqueo == 0)
            $timeout(function () { $dialog.modal('hide'); }, 100); //dentro de un timeout para que angular actualice la pantalla
    };
})


myApp.factory('myHttpInterceptor', function ($q, myService) {
    // factory retorna un objeto
    var myHttpInterceptor = {
        request: function (config) {
        //    myService.BloquearPantalla();
            return config;
        },
        requestError: function (config) {
            return config;
        },
        response: function (response) {
        //    myService.DesbloquearPantalla();
            return response;
        },
        responseError: function (response) {
       //     myService.DesbloquearPantalla();
            // acceso denegado generado por alguna llamada al servidor (no carga las vistas)
            if (response.status == 404 || response.status == 401) {
                myService.Alert("Acceso Denegado...");
            }
            else if (response.status == 400) {
                myService.Alert("Peticion incorrecta...");
            }
            else if (response.data && response.data.ExceptionMessage) {
                // error desde webapi
                myService.Alert(response.data.ExceptionMessage);
            }
            else {
                myService.Alert("Error en la aplicacion, reintente nuevamente.");
            }
            return $q.reject(response);
        }
    }
    return myHttpInterceptor;
});

// configura la app con el interceptor antes creado
myApp.config(function ($httpProvider) {
    //agrega el interceptor definido anteriormente
    $httpProvider.interceptors.push('myHttpInterceptor');
});


myApp.run(function ($rootScope, $http, $location, myService) {
    // $rootScope desde donde heredan todos los $scope de los controladores
    // todas las variables o funciones que se definan aquí están disponibles en todos los controladores
    $rootScope.TituloAccionABMC = { A: '(Agregar)', B: '(Eliminar)', M: '(Modificar)', C: '(Consultar)', L: '(Listado)' };
    $rootScope.AccionABMC = 'L';   // inicialmente inicia el el listado (buscar con parametros)
    $rootScope.Mensajes = { SD: ' No se encontraron registros...', RD: ' Revisar los campos ingresados...' };
});



myApp.controller("inicioCtrl", function ($scope) {
        $scope.Titulo = 'Web App Tarjetas';

    })

myApp.controller('TipoTarjetaCtrl',
    function ($scope, $http) {
        $scope.Titulo = 'Tipo Tarjeta';
        $http.get('/api/TipoTarjeta')
            .then(function (response) {
                $scope.Lista = response.data;
            });
    }
);

myApp.controller('TarjetasCtrl',
    function ($scope, $http, myService) {

        $scope.Titulo = 'Gestionar Tarjetas';  // inicia mostrando el Listado
        

        $scope.DtoFiltro = {};    // dto con las opciones para buscar en grilla
        $scope.DtoFiltro.Activo = null;
        $scope.PaginaActual = 1;  // inicia pagina 1

        // opciones del filtro activo
        $scope.OpcionesSiNo = [{ Id: null, Nombre: '' }, { Id: true, Nombre: 'SI' }, { Id: false, Nombre: 'NO' }];

        $http.get('/api/TipoTarjeta')
            .then(function (response) {
                $scope.TiposTarjetas = response.data;
        });

        ///**FUNCIONES**///
        $scope.Agregar = function () {
            $scope.AccionABMC = 'A';
            $scope.DtoSel = {};
            $scope.DtoSel.Activo = true;
            $scope.FormReg.$setUntouched();
            $scope.FormReg.$setPristine();  // restaura FormReg.$submitted = false

        };

        //Buscar segun los filtros, establecidos en DtoFiltro
        $scope.Buscar = function () {
            // las propiedades del params tienen que coincidir con el nombre de los parámetros de c# (case sensitive)
            params = { Nombre: $scope.DtoFiltro.Nombre, Activo: $scope.DtoFiltro.Activo, numeroPagina: $scope.PaginaActual };

            //myService.BloquearPantalla();

            $http.get('/api/Tarjetas', { params: params }).then(function (response) {
                    $scope.Lista = response.data.Lista;  // variable para luego imprimir
                    $scope.RegistrosTotal = response.data.RegistrosTotal;  // var para mostrar en interface
                    //myService.DesbloquearPantalla();  // cuando la fn termina exitosamente

                });
        };




        $scope.Consultar = function (Dto) {
            $scope.BuscarPorId(Dto, 'C');
        };

        //comienza la modificacion, luego la confirma con el metodo Grabar
        $scope.Modificar = function (Dto) {
            if (!Dto.Activo) {
                myService.Alert("No puede modificarse un registro Inactivo.");
                return;
            }
            $scope.FormReg.$setUntouched();
            $scope.FormReg.$setPristine();  // restaura FormReg.$submitted = false

            $scope.BuscarPorId(Dto, 'M');
        };

        //Obtengo datos del servidor de un registros, metodo usado en el consultar y modificar
        $scope.BuscarPorId = function (Dto, AccionABMC) {
            $http.get('/api/Tarjetas/' + Dto.IdTarjeta)
                .then(function (response) {
                    $scope.DtoSel = response.data;
                    //formatear fecha de  ISO 8061 a string dd/MM/yyyy
                    //var arrFecha = $scope.DtoSel.FechaAlta.substr(0, 10).split('-');
                    //$scope.DtoSel.FechaAlta = arrFecha[2] + '/' + arrFecha[1] + '/' + arrFecha[0];

                    //convertir fecha de formato ISO 8061 a fecha de javascript para el datepicker
                    $scope.DtoSel.FechaAlta = new Date($scope.DtoSel.FechaAlta);

                    $scope.AccionABMC = AccionABMC;
                });
        };


        //grabar tanto para altas como modificaciones
        $scope.Grabar = function () {
            //convertir fecha de string dd/MM/yyyy a ISO para que la entienda webapi
            //var arrFecha = $scope.DtoSel.FechaAlta.substr(0, 10).split('/');
            //if (arrFecha.length == 3)
            //    $scope.DtoSel.FechaAlta = new Date(arrFecha[2], arrFecha[1] - 1, arrFecha[0]).toISOString();

            if ($scope.DtoSel.IdTarjeta == undefined)  // agregar
            {
                $http.post('/api/Tarjetas/', $scope.DtoSel).then(function (response) {

                    $scope.Volver();
                    $scope.Buscar(); // vuelve a cargar las tarjetas desde el servidor
                    myService.Alert("Registro agregado correctamente.");
                });
            }
            else {
                $http.put('/api/Tarjetas/' + $scope.DtoSel.IdTarjeta, $scope.DtoSel).then(function (response) {
                    $scope.Volver();
                    $scope.Buscar(); // vuelve a cargar las tarjetas desde el servidor
                    myService.Alert("Registro modificado correctamente.")
                });
            }
        };








        // baja logica
        $scope.ActivarDesactivar = function (Dto) {
            //var resp = confirm("Esta seguro de " + (Dto.Activo ? "desactivar" : "activar") + " este registro?");
            //if (resp) {
            //    $http.delete('/api/Articulos/' + Dto.IdArticulo, Dto).then(function () {
            //        $scope.Buscar();  // vuelve a cargar los artículos desde el servidor
            //    });
            //}
                myService.Confirm("Esta seguro de " + (Dto.Activo ? "desactivar" : "activar") + " este registro?", fun, null, "Confirmación", "Aceptar", "Cancelar")
                function fun() {
                    $http.delete('/api/Tarjetas/' + Dto.IdTarjeta, Dto).then(function () {
                        $scope.Buscar();
                    });
                }

        };


        // Volver Agregar/Modificar
        $scope.Volver = function () {
            $scope.DtoSel = null;
            $scope.AccionABMC = 'L';
        };

        $scope.ImprimirListado = function () {
            myService.Alert("Sin desarrollar...")
        };

    }
);