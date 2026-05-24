/* =====================================================
   SITE.JS - Main Frontend JavaScript
   ===================================================== */

// DOM Helper Functions
const DOM = {
    get: (selector) => document.querySelector(selector),
    getAll: (selector) => document.querySelectorAll(selector),
    create: (tag, classes = []) => {
        const el = document.createElement(tag);
        if (classes.length) el.classList.add(...classes);
        return el;
    },
    addClass: (el, className) => el.classList.add(className),
    removeClass: (el, className) => el.classList.remove(className),
    toggleClass: (el, className) => el.classList.toggle(className),
    hasClass: (el, className) => el.classList.contains(className),
    on: (el, event, handler) => el.addEventListener(event, handler),
    off: (el, event, handler) => el.removeEventListener(event, handler),
    show: (el) => el.style.display = '',
    hide: (el) => el.style.display = 'none',
    remove: (el) => el.remove(),
};

// Notification/Toast System
const Notification = {
    queue: [],
    show: function(message, type = 'info', duration = 3000) {
        const id = Date.now();
        const notification = DOM.create('div', ['notification-toast', `notification-${type}`]);
        notification.id = `notification-${id}`;
        notification.innerHTML = `
            <div class="notification-content">
                <i class="icon-${type}"></i>
                <span>${message}</span>
            </div>
            <button class="notification-close">&times;</button>
        `;
        
        document.body.appendChild(notification);
        DOM.addClass(notification, 'notification-enter');
        
        const closeBtn = notification.querySelector('.notification-close');
        DOM.on(closeBtn, 'click', () => this.remove(id));
        
        if (duration) {
            setTimeout(() => this.remove(id), duration);
        }
        
        return id;
    },
    
    success: function(message, duration = 3000) {
        return this.show(message, 'success', duration);
    },
    
    error: function(message, duration = 5000) {
        return this.show(message, 'danger', duration);
    },
    
    info: function(message, duration = 3000) {
        return this.show(message, 'info', duration);
    },
    
    warning: function(message, duration = 4000) {
        return this.show(message, 'warning', duration);
    },
    
    remove: function(id) {
        const notification = DOM.get(`#notification-${id}`);
        if (notification) {
            DOM.addClass(notification, 'notification-exit');
            setTimeout(() => DOM.remove(notification), 250);
        }
    }
};

// Modal Management
const Modal = {
    open: function(modalId) {
        const modal = DOM.get(`#${modalId}`);
        const backdrop = modal.querySelector('.modal-backdrop');
        if (modal) {
            DOM.addClass(backdrop, 'show');
            DOM.addClass(modal, 'show');
            document.body.style.overflow = 'hidden';
        }
    },
    
    close: function(modalId) {
        const modal = DOM.get(`#${modalId}`);
        const backdrop = modal.querySelector('.modal-backdrop');
        if (modal) {
            DOM.removeClass(backdrop, 'show');
            DOM.removeClass(modal, 'show');
            document.body.style.overflow = '';
        }
    },
    
    init: function() {
        // Close on backdrop click
        DOM.getAll('.modal-backdrop').forEach(backdrop => {
            DOM.on(backdrop, 'click', (e) => {
                if (e.target === backdrop) {
                    const modal = backdrop.closest('.modal');
                    const modalId = modal?.id;
                    if (modalId) this.close(modalId);
                }
            });
        });
        
        // Close on close button click
        DOM.getAll('.modal-close').forEach(btn => {
            DOM.on(btn, 'click', () => {
                const modal = btn.closest('.modal');
                const modalId = modal?.id;
                if (modalId) this.close(modalId);
            });
        });
        
        // Close on ESC key
        DOM.on(document, 'keydown', (e) => {
            if (e.key === 'Escape') {
                const modal = DOM.get('.modal.show');
                if (modal) {
                    const modalId = modal.id;
                    this.close(modalId);
                }
            }
        });
    }
};

// Sidebar Navigation
const Sidebar = {
    init: function() {
        const toggle = DOM.get('.sidebar-toggle');
        const sidebar = DOM.get('.layout-sidebar');
        const overlay = DOM.get('.layout-sidebar-overlay');
        
        if (toggle) {
            DOM.on(toggle, 'click', () => {
                DOM.toggleClass(sidebar, 'open');
                DOM.toggleClass(overlay, 'visible');
            });
        }
        
        if (overlay) {
            DOM.on(overlay, 'click', () => {
                DOM.removeClass(sidebar, 'open');
                DOM.removeClass(overlay, 'visible');
            });
        }
        
        // Set active menu item
        const currentPath = window.location.pathname;
        DOM.getAll('.sidebar-menu-link').forEach(link => {
            if (link.getAttribute('href') === currentPath) {
                DOM.addClass(link, 'active');
            }
        });
    }
};

// Form Handling
const Form = {
    validate: function(formId) {
        const form = DOM.get(`#${formId}`);
        if (!form) return false;
        
        let isValid = true;
        const inputs = form.querySelectorAll('[required]');
        
        inputs.forEach(input => {
            if (!input.value.trim()) {
                this.setError(input, 'This field is required');
                isValid = false;
            } else {
                this.clearError(input);
            }
        });
        
        return isValid;
    },
    
    setError: function(input, message) {
        DOM.addClass(input, 'is-invalid');
        let errorEl = input.nextElementSibling;
        if (!errorEl?.classList.contains('form-error')) {
            errorEl = DOM.create('span', ['form-error']);
            input.parentNode.insertBefore(errorEl, input.nextSibling);
        }
        errorEl.textContent = message;
    },
    
    clearError: function(input) {
        DOM.removeClass(input, 'is-invalid');
        const errorEl = input.nextElementSibling;
        if (errorEl?.classList.contains('form-error')) {
            DOM.remove(errorEl);
        }
    }
};

// Table Utilities
const Table = {
    sort: function(columnIndex) {
        // Implementation for table sorting
        console.log('Sort by column:', columnIndex);
    },
    
    filter: function(query) {
        // Implementation for table filtering
        console.log('Filter table:', query);
    }
};

// Confirmation Dialog
const Confirm = {
    show: function(message, onConfirm, onCancel) {
        const modal = DOM.create('div', ['modal-backdrop', 'show']);
        modal.innerHTML = `
            <div class="modal">
                <div class="modal-header">
                    <h5 class="modal-title">Confirm Action</h5>
                    <button class="modal-close" type="button">&times;</button>
                </div>
                <div class="modal-body">
                    <p>${message}</p>
                </div>
                <div class="modal-footer">
                    <button class="btn btn-outline" id="confirm-cancel">Cancel</button>
                    <button class="btn btn-danger" id="confirm-ok">Confirm</button>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        document.body.style.overflow = 'hidden';
        
        const okBtn = modal.querySelector('#confirm-ok');
        const cancelBtn = modal.querySelector('#confirm-cancel');
        const closeBtn = modal.querySelector('.modal-close');
        
        const cleanup = () => {
            DOM.remove(modal);
            document.body.style.overflow = '';
        };
        
        DOM.on(okBtn, 'click', () => {
            cleanup();
            onConfirm?.();
        });
        
        DOM.on(cancelBtn, 'click', cleanup);
        DOM.on(closeBtn, 'click', cleanup);
    }
};

// AJAX Helper
const API = {
    async request(method, url, data = null) {
        try {
            const options = {
                method,
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                }
            };
            
            if (data) {
                options.body = JSON.stringify(data);
            }
            
            const response = await fetch(url, options);
            const result = await response.json();
            
            if (!response.ok) {
                throw new Error(result.message || 'An error occurred');
            }
            
            return result;
        } catch (error) {
            console.error('API Error:', error);
            Notification.error(error.message || 'An error occurred');
            throw error;
        }
    },
    
    get: function(url) {
        return this.request('GET', url);
    },
    
    post: function(url, data) {
        return this.request('POST', url, data);
    },
    
    put: function(url, data) {
        return this.request('PUT', url, data);
    },
    
    delete: function(url) {
        return this.request('DELETE', url);
    }
};

// Initialize on Document Ready
document.addEventListener('DOMContentLoaded', function() {
    // Initialize components
    Sidebar.init();
    Modal.init();
    
    // Set up global error handlers
    window.addEventListener('error', (e) => {
        console.error('Global error:', e.error);
        Notification.error('An unexpected error occurred');
    });
    
    // Add CSRF token to all AJAX requests if present
    const token = DOM.get('input[name="__RequestVerificationToken"]');
    if (token) {
        // Store CSRF token for API calls
        window.csrfToken = token.value;
    }
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { DOM, Notification, Modal, Sidebar, Form, Table, Confirm, API };
}
