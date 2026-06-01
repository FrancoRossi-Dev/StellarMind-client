const Toast = (() => {
  const DURATION = 5000;
  let container = null;

  function ensureContainer() {
    if (container) return container;
    container = document.createElement('div');
    container.id = 'toast-container';
    document.body.appendChild(container);
    return container;
  }

  const ICONS = {
    error:   'fa-circle-exclamation',
    success: 'fa-circle-check',
    warning: 'fa-triangle-exclamation',
    info:    'fa-circle-info',
  };

  function show(message, type = 'error', duration = DURATION) {
    const c = ensureContainer();
    const el = document.createElement('div');
    el.className = `toast toast--${type}`;
    el.setAttribute('role', 'alert');
    el.innerHTML =
      `<i class="fa-solid ${ICONS[type] ?? ICONS.error} toast__icon" aria-hidden="true"></i>` +
      `<span class="toast__msg">${message}</span>` +
      `<button class="toast__close" aria-label="Dismiss"><i class="fa-solid fa-xmark" aria-hidden="true"></i></button>`;

    el.querySelector('.toast__close').addEventListener('click', () => dismiss(el));
    c.appendChild(el);

    // Double rAF ensures browser paints initial state before transition fires
    requestAnimationFrame(() => requestAnimationFrame(() => el.classList.add('toast--visible')));

    if (duration > 0) setTimeout(() => dismiss(el), duration);
  }

  function dismiss(el) {
    el.classList.remove('toast--visible');
    el.addEventListener('transitionend', () => el.remove(), { once: true });
  }

  return { show };
})();
