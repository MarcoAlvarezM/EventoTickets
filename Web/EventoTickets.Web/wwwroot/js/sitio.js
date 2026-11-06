// Comportamientos mínimos de interfaz (sin lógica de negocio).
document.addEventListener("DOMContentLoaded", () => {
  // Mostrar / ocultar contraseña (pantallas de inicio de sesión y registro de Stitch)
  document.querySelectorAll("[data-alternar-clave]").forEach((boton) => {
    boton.addEventListener("click", () => {
      const campo = document.getElementById(boton.dataset.alternarClave);
      if (!campo) return;
      campo.type = campo.type === "password" ? "text" : "password";
      const icono = boton.querySelector(".material-symbols-outlined");
      if (icono) icono.textContent = campo.type === "password" ? "visibility" : "visibility_off";
    });
  });

  // Menú lateral del panel en pantallas pequeñas
  const lateral = document.getElementById("menu-lateral");
  document.querySelectorAll("[data-alternar-menu]").forEach((b) =>
    b.addEventListener("click", () => lateral && lateral.classList.toggle("-translate-x-full")));

  // Temporizador de reserva: muestra el tiempo restante hasta data-vence (ISO 8601)
  document.querySelectorAll("[data-vence]").forEach((nodo) => {
    const vence = new Date(nodo.dataset.vence).getTime();
    const pintar = () => {
      const resta = Math.max(0, vence - Date.now());
      const m = Math.floor(resta / 60000), s = Math.floor((resta % 60000) / 1000);
      nodo.textContent = resta > 0 ? `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")} min` : "Reserva vencida";
      if (resta === 0) { clearInterval(intervalo); nodo.closest("[data-contenedor-reserva]")?.classList.add("opacity-60"); }
    };
    const intervalo = setInterval(pintar, 1000);
    pintar();
  });

  // Los códigos QR son imágenes PNG generadas por el API (GET /api/entradas/{id}/qr, Módulo 4).

  // Vista previa del nombre del comprobante seleccionado
  document.querySelectorAll("input[type=file][data-vista-archivo]").forEach((campo) => {
    campo.addEventListener("change", () => {
      const destino = document.getElementById(campo.dataset.vistaArchivo);
      if (destino) destino.textContent = campo.files.length ? `${campo.files[0].name} · ${(campo.files[0].size / 1048576).toFixed(2)} MB` : "Ningún archivo seleccionado";
    });
  });

  // Confirmaciones de acciones destructivas
  document.querySelectorAll("form[data-confirmar]").forEach((f) =>
    f.addEventListener("submit", (e) => { if (!confirm(f.dataset.confirmar)) e.preventDefault(); }));

  // Imprimir (reportes / resumen)
  document.querySelectorAll("[data-imprimir]").forEach((b) => b.addEventListener("click", () => window.print()));
});
