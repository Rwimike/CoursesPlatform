const API_URL = 'http://localhost:5000/api';

const { createApp } = Vue;

createApp({
    data() {
        return {
            isAuthenticated: false,
            userEmail: '',
            token: '',
            currentView: 'dashboard',

            loginData: {
                email: '',
                password: ''
            },
            loginError: '',

            courses: [],
            lessons: [],
            selectedCourse: null,

            filters: {
                search: '',
                status: ''
            },

            pagination: {
                currentPage: 1,
                pageSize: 9,
                totalPages: 0
            },

            metrics: {
                totalCourses: 0,
                publishedCourses: 0,
                draftCourses: 0,
                totalLessons: 0,
                recentCourses: []
            },

            modals: {
                course: false,
                lessons: false,
                lessonForm: false
            },

            courseForm: {
                title: ''
            },
            editingCourse: null,

            lessonForm: {
                title: '',
                order: 1
            },
            editingLesson: null
        };
    },

    mounted() {
        const token = localStorage.getItem('token');
        const email = localStorage.getItem('email');

        if (token && email) {
            this.token = token;
            this.userEmail = email;
            this.isAuthenticated = true;
            this.loadMetrics();
        }
    },

    methods: {
        // Auth
        async login() {
            try {
                const response = await axios.post(`${API_URL}/auth/login`, this.loginData);
                this.token = response.data.token;
                this.userEmail = response.data.email;
                this.isAuthenticated = true;

                localStorage.setItem('token', this.token);
                localStorage.setItem('email', this.userEmail);

                this.loginError = '';
                this.loadMetrics();
            } catch (error) {
                this.loginError = error.response?.data?.message || 'Error al iniciar sesión';
            }
        },

        logout() {
            this.isAuthenticated = false;
            this.token = '';
            this.userEmail = '';
            localStorage.removeItem('token');
            localStorage.removeItem('email');
        },

        getAuthHeaders() {
            return {
                headers: {
                    'Authorization': `Bearer ${this.token}`
                }
            };
        },

        // Metrics
        async loadMetrics() {
            try {
                const response = await axios.get(
                    `${API_URL}/courses/search?page=1&pageSize=100`,
                    this.getAuthHeaders()
                );

                const allCourses = response.data.items;

                this.metrics.totalCourses = allCourses.length;
                this.metrics.publishedCourses = allCourses.filter(c => c.status === 1).length;
                this.metrics.draftCourses = allCourses.filter(c => c.status === 0).length;

                // Get recent courses
                this.metrics.recentCourses = allCourses
                    .sort((a, b) => new Date(b.updatedAt) - new Date(a.updatedAt))
                    .slice(0, 5);

                // Count total lessons
                let totalLessons = 0;
                for (const course of allCourses) {
                    const lessonsResponse = await axios.get(
                        `${API_URL}/courses/${course.id}/lessons`,
                        this.getAuthHeaders()
                    );
                    totalLessons += lessonsResponse.data.length;
                }
                this.metrics.totalLessons = totalLessons;

            } catch (error) {
                console.error('Error loading metrics:', error);
            }
        },

        // Courses
        async loadCourses() {
            try {
                const params = new URLSearchParams({
                    page: this.pagination.currentPage,
                    pageSize: this.pagination.pageSize
                });

                if (this.filters.search) {
                    params.append('q', this.filters.search);
                }
                if (this.filters.status !== '') {
                    params.append('status', this.filters.status);
                }

                const response = await axios.get(
                    `${API_URL}/courses/search?${params}`,
                    this.getAuthHeaders()
                );

                this.courses = response.data.items;
                this.pagination.totalPages = response.data.totalPages;
            } catch (error) {
                alert('Error al cargar cursos');
                console.error(error);
            }
        },

        changePage(page) {
            this.pagination.currentPage = page;
            this.loadCourses();
        },

        showCreateCourseModal() {
            this.editingCourse = null;
            this.courseForm.title = '';
            this.modals.course = true;
        },

        editCourse(course) {
            this.editingCourse = course;
            this.courseForm.title = course.title;
            this.modals.course = true;
        },

        async saveCourse() {
            try {
                if (this.editingCourse) {
                    await axios.put(
                        `${API_URL}/courses/${this.editingCourse.id}`,
                        { title: this.courseForm.title },
                        this.getAuthHeaders()
                    );
                } else {
                    await axios.post(
                        `${API_URL}/courses`,
                        { title: this.courseForm.title },
                        this.getAuthHeaders()
                    );
                }

                this.modals.course = false;
                this.loadCourses();
                this.loadMetrics();
            } catch (error) {
                alert('Error al guardar curso');
                console.error(error);
            }
        },

        async deleteCourse(id) {
            if (!confirm('¿Eliminar este curso?')) return;

            try {
                await axios.delete(`${API_URL}/courses/${id}`, this.getAuthHeaders());
                this.loadCourses();
                this.loadMetrics();
            } catch (error) {
                alert('Error al eliminar curso');
                console.error(error);
            }
        },

        async publishCourse(id) {
            try {
                await axios.patch(
                    `${API_URL}/courses/${id}/publish`,
                    {},
                    this.getAuthHeaders()
                );
                this.loadCourses();
                this.loadMetrics();
            } catch (error) {
                alert(error.response?.data?.message || 'Error al publicar curso');
                console.error(error);
            }
        },

        async unpublishCourse(id) {
            try {
                await axios.patch(
                    `${API_URL}/courses/${id}/unpublish`,
                    {},
                    this.getAuthHeaders()
                );
                this.loadCourses();
                this.loadMetrics();
            } catch (error) {
                alert('Error al despublicar curso');
                console.error(error);
            }
        },

        // Lessons
        async viewLessons(course) {
            this.selectedCourse = course;
            await this.loadLessons();
            this.modals.lessons = true;
        },

        async loadLessons() {
            try {
                const response = await axios.get(
                    `${API_URL}/courses/${this.selectedCourse.id}/lessons`,
                    this.getAuthHeaders()
                );
                this.lessons = response.data;
            } catch (error) {
                alert('Error al cargar lecciones');
                console.error(error);
            }
        },

        showCreateLessonModal() {
            this.editingLesson = null;
            this.lessonForm.title = '';
            this.lessonForm.order = this.lessons.length + 1;
            this.modals.lessonForm = true;
        },

        editLesson(lesson) {
            this.editingLesson = lesson;
            this.lessonForm.title = lesson.title;
            this.lessonForm.order = lesson.order;
            this.modals.lessonForm = true;
        },

        async saveLesson() {
            try {
                if (this.editingLesson) {
                    await axios.put(
                        `${API_URL}/courses/${this.selectedCourse.id}/lessons/${this.editingLesson.id}`,
                        this.lessonForm,
                        this.getAuthHeaders()
                    );
                } else {
                    await axios.post(
                        `${API_URL}/courses/${this.selectedCourse.id}/lessons`,
                        this.lessonForm,
                        this.getAuthHeaders()
                    );
                }

                this.modals.lessonForm = false;
                await this.loadLessons();
                this.loadMetrics();
            } catch (error) {
                alert(error.response?.data?.message || 'Error al guardar lección');
                console.error(error);
            }
        },

        async deleteLesson(id) {
            if (!confirm('¿Eliminar esta lección?')) return;

            try {
                await axios.delete(
                    `${API_URL}/courses/${this.selectedCourse.id}/lessons/${id}`,
                    this.getAuthHeaders()
                );
                await this.loadLessons();
                this.loadMetrics();
            } catch (error) {
                alert('Error al eliminar lección');
                console.error(error);
            }
        },

        async reorderLesson(id, moveUp) {
            try {
                await axios.patch(
                    `${API_URL}/courses/${this.selectedCourse.id}/lessons/${id}/reorder`,
                    { moveUp },
                    this.getAuthHeaders()
                );
                await this.loadLessons();
            } catch (error) {
                alert('No se puede reordenar más en esa dirección');
                console.error(error);
            }
        },

        // Utilities
        formatDate(date) {
            return new Date(date).toLocaleDateString('es-ES', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        }
    }
}).mount('#app');