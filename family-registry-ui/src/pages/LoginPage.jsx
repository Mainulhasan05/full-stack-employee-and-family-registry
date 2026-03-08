import { useState } from 'react';
import { Card, Form, Input, Button, Typography, message, Tabs, Select } from 'antd';
import { UserOutlined, LockOutlined, MailOutlined, IdcardOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const { Title, Text } = Typography;

export default function LoginPage() {
    const [loading, setLoading] = useState(false);
    const { login, register } = useAuth();
    const navigate = useNavigate();

    const handleLogin = async (values) => {
        setLoading(true);
        try {
            await login(values.userName, values.password);
            message.success('Login successful!');
            navigate('/');
        } catch (err) {
            message.error(err.response?.data?.message || 'Login failed');
        } finally {
            setLoading(false);
        }
    };

    const handleRegister = async (values) => {
        setLoading(true);
        try {
            await register(values.userName, values.email, values.fullName, values.password, values.role);
            message.success('Registration successful!');
            navigate('/');
        } catch (err) {
            const errors = err.response?.data?.errors;
            if (errors && Array.isArray(errors)) {
                errors.forEach((e) => message.error(e));
            } else {
                message.error(err.response?.data?.message || 'Registration failed');
            }
        } finally {
            setLoading(false);
        }
    };

    const tabItems = [
        {
            key: 'login',
            label: 'Login',
            children: (
                <Form layout="vertical" onFinish={handleLogin} autoComplete="off">
                    <Form.Item
                        name="userName"
                        rules={[{ required: true, message: 'Please enter your username' }]}
                    >
                        <Input prefix={<UserOutlined />} placeholder="Username" size="large" />
                    </Form.Item>
                    <Form.Item
                        name="password"
                        rules={[{ required: true, message: 'Please enter your password' }]}
                    >
                        <Input.Password prefix={<LockOutlined />} placeholder="Password" size="large" />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit" loading={loading} block size="large">
                            Sign In
                        </Button>
                    </Form.Item>
                    <div style={{ textAlign: 'center', color: '#888' }}>
                        <Text type="secondary">
                            Demo: <strong>admin / Admin@123</strong> or <strong>viewer / Viewer@123</strong>
                        </Text>
                    </div>
                </Form>
            ),
        },
        {
            key: 'register',
            label: 'Register',
            children: (
                <Form layout="vertical" onFinish={handleRegister} autoComplete="off" initialValues={{ role: 'Viewer' }}>
                    <Form.Item
                        name="fullName"
                        rules={[{ required: true, message: 'Please enter your full name' }]}
                    >
                        <Input prefix={<IdcardOutlined />} placeholder="Full Name" size="large" />
                    </Form.Item>
                    <Form.Item
                        name="userName"
                        rules={[{ required: true, message: 'Please enter a username' }]}
                    >
                        <Input prefix={<UserOutlined />} placeholder="Username" size="large" />
                    </Form.Item>
                    <Form.Item
                        name="email"
                        rules={[
                            { required: true, message: 'Please enter your email' },
                            { type: 'email', message: 'Please enter a valid email' },
                        ]}
                    >
                        <Input prefix={<MailOutlined />} placeholder="Email" size="large" />
                    </Form.Item>
                    <Form.Item
                        name="password"
                        rules={[{ required: true, message: 'Please enter a password' }]}
                    >
                        <Input.Password prefix={<LockOutlined />} placeholder="Password" size="large" />
                    </Form.Item>
                    <Form.Item
                        name="role"
                        label="Role"
                    >
                        <Select size="large">
                            <Select.Option value="Viewer">Viewer</Select.Option>
                            <Select.Option value="Admin">Admin</Select.Option>
                        </Select>
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit" loading={loading} block size="large">
                            Register
                        </Button>
                    </Form.Item>
                </Form>
            ),
        },
    ];

    return (
        <div
            style={{
                minHeight: '100vh',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            }}
        >
            <Card
                style={{
                    width: 420,
                    borderRadius: 12,
                    boxShadow: '0 20px 60px rgba(0,0,0,0.3)',
                }}
            >
                <div style={{ textAlign: 'center', marginBottom: 24 }}>
                    <Title level={3} style={{ margin: 0, color: '#1677ff' }}>
                        🏢 Employee Registry
                    </Title>
                    <Text type="secondary">Bangladesh Employee Management System</Text>
                </div>
                <Tabs items={tabItems} centered />
            </Card>
        </div>
    );
}
