const onlineWatchers = new Map();

export function isOnline() {
    return navigator.onLine !== false;
}

export function getItem(key) {
    return localStorage.getItem(key);
}

export function setItem(key, value) {
    localStorage.setItem(key, value);
}

export function removeItem(key) {
    localStorage.removeItem(key);
}

export function watchOnline(dotNetRef) {
    const handler = () => dotNetRef.invokeMethodAsync('OnConnectionRestored');
    window.addEventListener('online', handler);
    const id = crypto.randomUUID();
    onlineWatchers.set(id, { handler });
    return id;
}

export function unwatchOnline(id) {
    const watcher = onlineWatchers.get(id);
    if (!watcher) return;
    window.removeEventListener('online', watcher.handler);
    onlineWatchers.delete(id);
}