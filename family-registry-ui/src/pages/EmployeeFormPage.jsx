import { useState, useEffect } from 'react';
import {
    Card,
    Form,
    Input,
    InputNumber,
    Button,
    DatePicker,
    Space,
    Typography,
    message,
    Divider,
    Switch,
    Row,
    Col,
} from 'antd';
import {
    PlusOutlined,
    MinusCircleOutlined,
    SaveOutlined,
    ArrowLeftOutlined,
} from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../api/axios';
import dayjs from 'dayjs';

const { Title, Text } = Typography;

export default function EmployeeFormPage() {
    const [form] = Form.useForm();
    const [loading, setLoading] = useState(false);
    const [hasSpouse, setHasSpouse] = useState(false);
    const navigate = useNavigate();
    const { id } = useParams();
    const isEdit = Boolean(id);

    useEffect(() => {
        if (isEdit) {
            fetchEmployee();
        }
    }, [id]);

    const fetchEmployee = async () => {
        try {
            const res = await api.get(`/employees/${id}`);
            const emp = res.data;
            const formValues = {
                name: emp.name,
                nid: emp.nid,
                phone: emp.phone,
                department: emp.department,
                basicSalary: emp.basicSalary,
                children: emp.children?.map((c) => ({
                    name: c.name,
                    dateOfBirth: dayjs(c.dateOfBirth),
                })) || [],
            };
            if (emp.spouse) {
                setHasSpouse(true);
                formValues.spouse = {
                    name: emp.spouse.name,
                    nid: emp.spouse.nid,
                };
            }
            form.setFieldsValue(formValues);
        } catch (err) {
            message.error('Failed to load employee');
            navigate('/');
        }
    };

    const handleSubmit = async (values) => {
        setLoading(true);
        try {
            const payload = {
                name: values.name,
                nid: values.nid,
                phone: values.phone,
                department: values.department,
                basicSalary: values.basicSalary,
                spouse: hasSpouse && values.spouse?.name
                    ? { name: values.spouse.name, nid: values.spouse.nid }
                    : null,
                children: (values.children || []).map((c) => ({
                    name: c.name,
                    dateOfBirth: c.dateOfBirth.toISOString(),
                })),
            };

            if (isEdit) {
                await api.put(`/employees/${id}`, payload);
                message.success('Employee updated');
            } else {
                await api.post('/employees', payload);
                message.success('Employee created');
            }
            navigate('/');
        } catch (err) {
            const msg = err.response?.data?.message || err.response?.data?.title || 'Operation failed';
            message.error(msg);
            // Show validation errors
            if (err.response?.data?.errors) {
                const errors = err.response.data.errors;
                Object.values(errors).flat().forEach((e) => message.error(e));
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ maxWidth: 800, margin: '0 auto' }}>
            <Button
                type="text"
                icon={<ArrowLeftOutlined />}
                onClick={() => navigate('/')}
                style={{ marginBottom: 16 }}
            >
                Back to List
            </Button>

            <Card style={{ borderRadius: 12 }}>
                <Title level={3} style={{ marginBottom: 24 }}>
                    {isEdit ? '✏️ Edit Employee' : '➕ Add New Employee'}
                </Title>

                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleSubmit}
                    initialValues={{ children: [] }}
                    scrollToFirstError
                >
                    {/* Employee Section */}
                    <Title level={5} style={{ color: '#1677ff' }}>
                        Personal Information
                    </Title>
                    <Row gutter={16}>
                        <Col xs={24} sm={12}>
                            <Form.Item
                                name="name"
                                label="Full Name"
                                rules={[
                                    { required: true, message: 'Name is required' },
                                    { max: 100, message: 'Max 100 characters' },
                                ]}
                            >
                                <Input placeholder="e.g. Md. Hasan Mahmud" />
                            </Form.Item>
                        </Col>
                        <Col xs={24} sm={12}>
                            <Form.Item
                                name="nid"
                                label="NID (National ID)"
                                rules={[
                                    { required: true, message: 'NID is required' },
                                    {
                                        pattern: /^(\d{10}|\d{17})$/,
                                        message: 'NID must be 10 or 17 digits',
                                    },
                                ]}
                            >
                                <Input placeholder="10 or 17 digit NID" />
                            </Form.Item>
                        </Col>
                    </Row>
                    <Row gutter={16}>
                        <Col xs={24} sm={12}>
                            <Form.Item
                                name="phone"
                                label="Phone Number"
                                rules={[
                                    { required: true, message: 'Phone is required' },
                                    {
                                        pattern: /^(\+880\d{10}|01\d{9})$/,
                                        message: 'Must be valid BD format (+880XXXXXXXXXX or 01XXXXXXXXX)',
                                    },
                                ]}
                            >
                                <Input placeholder="+8801712345678 or 01712345678" />
                            </Form.Item>
                        </Col>
                        <Col xs={24} sm={12}>
                            <Form.Item
                                name="department"
                                label="Department"
                                rules={[{ required: true, message: 'Department is required' }]}
                            >
                                <Input placeholder="e.g. Engineering" />
                            </Form.Item>
                        </Col>
                    </Row>
                    <Row gutter={16}>
                        <Col xs={24} sm={12}>
                            <Form.Item
                                name="basicSalary"
                                label="Basic Salary (BDT)"
                                rules={[
                                    { required: true, message: 'Salary is required' },
                                    { type: 'number', min: 1, message: 'Must be greater than 0' },
                                ]}
                            >
                                <InputNumber
                                    style={{ width: '100%' }}
                                    placeholder="e.g. 55000"
                                    formatter={(value) => `৳ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                                    parser={(value) => value.replace(/৳\s?|(,*)/g, '')}
                                />
                            </Form.Item>
                        </Col>
                    </Row>

                    <Divider />

                    {/* Spouse Section */}
                    <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 16 }}>
                        <Title level={5} style={{ color: '#52c41a', margin: 0 }}>
                            Spouse Information
                        </Title>
                        <Switch
                            checked={hasSpouse}
                            onChange={setHasSpouse}
                            checkedChildren="Has Spouse"
                            unCheckedChildren="No Spouse"
                        />
                    </div>

                    {hasSpouse && (
                        <Row gutter={16}>
                            <Col xs={24} sm={12}>
                                <Form.Item
                                    name={['spouse', 'name']}
                                    label="Spouse Name"
                                    rules={[{ required: hasSpouse, message: 'Spouse name is required' }]}
                                >
                                    <Input placeholder="e.g. Fatema Akter" />
                                </Form.Item>
                            </Col>
                            <Col xs={24} sm={12}>
                                <Form.Item
                                    name={['spouse', 'nid']}
                                    label="Spouse NID"
                                    rules={[
                                        { required: hasSpouse, message: 'Spouse NID is required' },
                                        {
                                            pattern: /^(\d{10}|\d{17})$/,
                                            message: 'NID must be 10 or 17 digits',
                                        },
                                    ]}
                                >
                                    <Input placeholder="10 or 17 digit NID" />
                                </Form.Item>
                            </Col>
                        </Row>
                    )}

                    <Divider />

                    {/* Children Section */}
                    <Title level={5} style={{ color: '#fa8c16', marginBottom: 16 }}>
                        Children Information
                    </Title>

                    <Form.List name="children">
                        {(fields, { add, remove }) => (
                            <>
                                {fields.map(({ key, name, ...restField }) => (
                                    <Card
                                        key={key}
                                        size="small"
                                        style={{
                                            marginBottom: 12,
                                            borderRadius: 8,
                                            borderColor: '#f0f0f0',
                                            background: '#fafafa',
                                        }}
                                        extra={
                                            <Button
                                                type="text"
                                                danger
                                                icon={<MinusCircleOutlined />}
                                                onClick={() => remove(name)}
                                            >
                                                Remove
                                            </Button>
                                        }
                                    >
                                        <Row gutter={16}>
                                            <Col xs={24} sm={12}>
                                                <Form.Item
                                                    {...restField}
                                                    name={[name, 'name']}
                                                    label="Child Name"
                                                    rules={[{ required: true, message: 'Name is required' }]}
                                                >
                                                    <Input placeholder="e.g. Arif Hasan" />
                                                </Form.Item>
                                            </Col>
                                            <Col xs={24} sm={12}>
                                                <Form.Item
                                                    {...restField}
                                                    name={[name, 'dateOfBirth']}
                                                    label="Date of Birth"
                                                    rules={[{ required: true, message: 'Date of birth is required' }]}
                                                >
                                                    <DatePicker
                                                        style={{ width: '100%' }}
                                                        disabledDate={(current) => current && current > dayjs()}
                                                    />
                                                </Form.Item>
                                            </Col>
                                        </Row>
                                    </Card>
                                ))}
                                <Button
                                    type="dashed"
                                    onClick={() => add()}
                                    block
                                    icon={<PlusOutlined />}
                                    style={{ borderRadius: 8 }}
                                >
                                    Add Child
                                </Button>
                            </>
                        )}
                    </Form.List>

                    <Divider />

                    <Space>
                        <Button
                            type="primary"
                            htmlType="submit"
                            loading={loading}
                            icon={<SaveOutlined />}
                            size="large"
                        >
                            {isEdit ? 'Update Employee' : 'Create Employee'}
                        </Button>
                        <Button size="large" onClick={() => navigate('/')}>
                            Cancel
                        </Button>
                    </Space>
                </Form>
            </Card>
        </div>
    );
}
